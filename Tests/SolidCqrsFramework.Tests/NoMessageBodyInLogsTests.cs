using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SolidCqrsFramework.Commanding;
using SolidCqrsFramework.EventManagement;
using SolidCqrsFramework.Exceptions;
using Xunit;

// MessageBodyLogging.Enabled is process-wide; tests that flip it must not overlap with the others.
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace SolidCqrsFramework.Tests
{
    /// <summary>
    /// Commands and events carry personal data and CloudWatch keeps every log line (ExpertsCircle/ExpersCircleCandidate#517).
    /// These tests capture every line the framework logs (all categories, Trace and up, message and exception) and look
    /// for the body's values. Error lines must still carry the exception, its message and the level the log monitors read.
    /// </summary>
    public class NoMessageBodyInLogsTests : IDisposable
    {
        private const string Email = "sam.candidate@acme.example";
        private const string InvoiceEmail = "accounts@acme.example";
        private const string InvoiceAddress = "1 Invoice Street, Leeds LS1 1AA";
        private const string Phone = "+44 7700 900123";
        private const string Notes = "Started on 3 March after a short notice period";
        private const string CandidateName = "Sam Okafor";
        private const string CvFileName = "sam-okafor-cv.pdf";

        private readonly CapturingLoggerProvider _logs = new CapturingLoggerProvider();

        public NoMessageBodyInLogsTests() => MessageBodyLogging.Enabled = false;

        public void Dispose() => MessageBodyLogging.Enabled = false;

        [Fact]
        public async Task CommandBus_logs_the_command_name_not_its_body()
        {
            var bus = BuildCommandBus(new RecordingCommandHandler());

            await bus.Handle(Command());

            Assert.Contains(_logs.Lines, l => l.Text.Contains("Handling Command details") && l.Text.Contains(nameof(RecordHireForTest)));
            Assert.Contains(_logs.Lines, l => l.Text.Contains("Command handling was successful"));
            AssertNoBody();
        }

        [Fact]
        public async Task CommandBus_error_keeps_the_exception_message_without_the_body()
        {
            var bus = BuildCommandBus(new RecordingCommandHandler(new InvalidOperationException("Invoice service unavailable")));

            await Assert.ThrowsAsync<InvalidOperationException>(() => bus.Handle(Command()));

            var error = Assert.Single(_logs.Lines, l => l.Level == LogLevel.Error);
            Assert.Contains("Invoice service unavailable", error.Text);
            Assert.Contains(nameof(RecordHireForTest), error.Text);
            Assert.IsType<InvalidOperationException>(error.Exception);
            AssertNoBody();
        }

        [Fact]
        public async Task CommandBus_domain_rule_warning_has_no_body()
        {
            var bus = BuildCommandBus(new RecordingCommandHandler(new DomainException("The hire is already recorded")));

            await Assert.ThrowsAsync<DomainException>(() => bus.Handle(Command()));

            Assert.Contains(_logs.Lines, l => l.Level == LogLevel.Warning && l.Text.Contains("The hire is already recorded"));
            AssertNoBody();
        }

        [Fact]
        public async Task EventProcessor_logs_the_event_name_and_ids_not_its_body()
        {
            var @event = Event();
            var processor = BuildEventProcessor(new RecordingEventHandler());

            await processor.ProcessEventAsync(typeof(HireRecordedForTest), @event);

            var processing = Assert.Single(_logs.Lines, l => l.Text.Contains("Processing Event in EventProcessor"));
            Assert.Contains(nameof(HireRecordedForTest), processing.Text);
            Assert.Contains("HIRE-1", processing.Text);
            Assert.Contains(@event.Id.ToString(), processing.Text);
            Assert.Contains(_logs.Lines, l => l.Text.Contains("Handling event in event handler")
                                              && l.Text.Contains(nameof(RecordingEventHandler)) && l.Text.Contains("HIRE-1"));
            Assert.Contains(_logs.Lines, l => l.Text.Contains("Event handling Successful"));
            AssertNoBody();
        }

        [Fact]
        public async Task Handler_failure_is_logged_as_an_error_with_the_exception_and_without_the_body()
        {
            var processor = BuildEventProcessor(new RecordingEventHandler(new InvalidOperationException("Downstream call failed")));

            // EventProcessor swallows a plain handler exception (see CLAUDE.md); the decorator's Error line is the only trace.
            await processor.ProcessEventAsync(typeof(HireRecordedForTest), Event());

            var error = Assert.Single(_logs.Lines, l => l.Level == LogLevel.Error);
            Assert.Contains("Downstream call failed", error.Text);
            Assert.Contains(nameof(HireRecordedForTest), error.Text);
            Assert.Contains(nameof(RecordingEventHandler), error.Text);
            Assert.Contains("HIRE-1", error.Text);
            Assert.IsType<InvalidOperationException>(error.Exception);
            AssertNoBody();
        }

        [Fact]
        public async Task Handler_domain_exception_warnings_have_no_body()
        {
            var processor = BuildEventProcessor(new RecordingEventHandler(new DomainException("Hire already reported")));

            await Assert.ThrowsAsync<DomainException>(() => processor.ProcessEventAsync(typeof(HireRecordedForTest), Event()));

            Assert.Contains(_logs.Lines, l => l.Level == LogLevel.Warning && l.Text.Contains("Hire already reported"));
            AssertNoBody();
        }

        [Fact]
        public async Task InMemoryEventBus_error_names_the_handler_without_the_body()
        {
            var services = Services();
            services.AddSingleton<IEventHandler<HireRecordedForTest>>(new RecordingEventHandler(new InvalidOperationException("Projection write failed")));
            services.AddSingleton<InMemoryEventBus>();
            var bus = services.BuildServiceProvider().GetRequiredService<InMemoryEventBus>();

            await Assert.ThrowsAsync<InvalidOperationException>(() => bus.Publish(new[] { Event() }));

            var error = Assert.Single(_logs.Lines, l => l.Level == LogLevel.Error);
            Assert.Contains(nameof(RecordingEventHandler), error.Text);
            Assert.Contains("HIRE-1", error.Text);
            AssertNoBody();
        }

        [Fact]
        public async Task Bodies_are_logged_only_when_opted_in()
        {
            MessageBodyLogging.Enabled = true;

            await BuildEventProcessor(new RecordingEventHandler()).ProcessEventAsync(typeof(HireRecordedForTest), Event());
            await BuildCommandBus(new RecordingCommandHandler()).Handle(Command());

            // Proves the assertions above would catch a body: with the opt-in on, it is there.
            Assert.Contains(_logs.Lines, l => l.Text.Contains("Processing Event in EventProcessor") && l.Text.Contains(InvoiceEmail));
            Assert.Contains(_logs.Lines, l => l.Text.Contains("Handling Command details") && l.Text.Contains(InvoiceEmail));
        }

        [Theory]
        [InlineData(null, null, false)]
        [InlineData("false", null, false)]
        [InlineData("true", null, true)]
        [InlineData("TRUE", null, true)]
        [InlineData("1", null, true)]
        [InlineData("true", "ec-candidate-fulfillment", false)]
        public void The_environment_opt_in_is_ignored_inside_Lambda(string optIn, string lambdaName, bool expected)
        {
            var variables = new Dictionary<string, string>
            {
                [MessageBodyLogging.EnvironmentVariable] = optIn,
                ["AWS_LAMBDA_FUNCTION_NAME"] = lambdaName
            };

            Assert.Equal(expected, MessageBodyLogging.FromEnvironment(name => variables.TryGetValue(name, out var v) ? v : null));
        }

        private void AssertNoBody()
        {
            Assert.NotEmpty(_logs.Lines);
            var text = string.Join("\n", _logs.Lines.Select(l => l.Text + " " + l.Exception));
            foreach (var value in new[] { Email, InvoiceEmail, InvoiceAddress, Phone, Notes, CandidateName, CvFileName, "@acme.example" })
                Assert.DoesNotContain(value, text);
        }

        private static RecordHireForTest Command() => new RecordHireForTest
        {
            HireId = "HIRE-1",
            CandidateName = CandidateName,
            CandidateEmail = Email,
            CandidatePhone = Phone,
            CvFileName = CvFileName,
            InvoiceEmail = InvoiceEmail,
            InvoiceAddress = InvoiceAddress,
            Notes = Notes
        };

        private static HireRecordedForTest Event() =>
            new HireRecordedForTest("HIRE-1", DateTime.UtcNow, CandidateName, Email, Phone, CvFileName, InvoiceEmail, InvoiceAddress, Notes);

        private IServiceCollection Services()
        {
            var services = new ServiceCollection();
            services.AddLogging(b => b.ClearProviders().AddProvider(_logs).SetMinimumLevel(LogLevel.Trace));
            return services;
        }

        private ICommandBus BuildCommandBus(ICommandHandler<RecordHireForTest> handler)
        {
            var services = Services();
            services.AddSingleton(handler);
            services.AddSingleton<ICommandBus, CommandBus>();
            return services.BuildServiceProvider().GetRequiredService<ICommandBus>();
        }

        private EventProcessor BuildEventProcessor(IEventHandler<HireRecordedForTest> handler)
        {
            var services = Services();
            services.AddSingleton(handler);
            // No CloudWatchMetricRecorder registered: the metric call after a successful handler fails and EventProcessor
            // swallows it, which does not affect what is logged before it.
            return new EventProcessor(services.BuildServiceProvider(), isInMemory: false);
        }

        public class RecordHireForTest : Command
        {
            public string HireId { get; set; }
            public string CandidateName { get; set; }
            public string CandidateEmail { get; set; }
            public string CandidatePhone { get; set; }
            public string CvFileName { get; set; }
            public string InvoiceEmail { get; set; }
            public string InvoiceAddress { get; set; }
            public string Notes { get; set; }
        }

        public class HireRecordedForTest : Event
        {
            public HireRecordedForTest(string aggregateId, DateTime date, string candidateName, string candidateEmail, string candidatePhone,
                string cvFileName, string invoiceEmail, string invoiceAddress, string notes) : base(aggregateId, date)
            {
                CandidateName = candidateName;
                CandidateEmail = candidateEmail;
                CandidatePhone = candidatePhone;
                CvFileName = cvFileName;
                InvoiceEmail = invoiceEmail;
                InvoiceAddress = invoiceAddress;
                Notes = notes;
            }

            public string CandidateName { get; }
            public string CandidateEmail { get; }
            public string CandidatePhone { get; }
            public string CvFileName { get; }
            public string InvoiceEmail { get; }
            public string InvoiceAddress { get; }
            public string Notes { get; }
        }

        // Public: the framework calls handlers through `dynamic`, which cannot see private types.
        public class RecordingCommandHandler : ICommandHandler<RecordHireForTest>
        {
            private readonly Exception _throw;
            public RecordingCommandHandler(Exception toThrow = null) => _throw = toThrow;
            public Task ExecuteAsync(RecordHireForTest command) => _throw == null ? Task.CompletedTask : Task.FromException(_throw);
        }

        public class RecordingEventHandler : IEventHandler<HireRecordedForTest>
        {
            private readonly Exception _throw;
            public RecordingEventHandler(Exception toThrow = null) => _throw = toThrow;
            public Task Handle(HireRecordedForTest handle) => _throw == null ? Task.CompletedTask : Task.FromException(_throw);
        }

        public sealed class LogLine
        {
            public LogLevel Level { get; init; }
            public string Category { get; init; }
            public string Text { get; init; }
            public Exception Exception { get; init; }
            public override string ToString() => $"{Level} {Category}: {Text} {Exception}";
        }

        private sealed class CapturingLoggerProvider : ILoggerProvider
        {
            private readonly ConcurrentQueue<LogLine> _lines = new ConcurrentQueue<LogLine>();
            public IReadOnlyList<LogLine> Lines => _lines.ToList();
            public ILogger CreateLogger(string categoryName) => new CapturingLogger(categoryName, _lines);
            public void Dispose() { }

            private sealed class CapturingLogger : ILogger
            {
                private readonly string _category;
                private readonly ConcurrentQueue<LogLine> _lines;

                public CapturingLogger(string category, ConcurrentQueue<LogLine> lines)
                {
                    _category = category;
                    _lines = lines;
                }

                public IDisposable BeginScope<TState>(TState state) where TState : notnull
                {
                    // Scopes reach CloudWatch too when a provider includes them: record them as lines.
                    _lines.Enqueue(new LogLine { Level = LogLevel.None, Category = _category, Text = "scope: " + state });
                    return null;
                }

                public bool IsEnabled(LogLevel logLevel) => true;

                public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) =>
                    _lines.Enqueue(new LogLine { Level = logLevel, Category = _category, Text = formatter(state, exception), Exception = exception });
            }
        }
    }
}
