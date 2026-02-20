// DESAFIO: Sistema de Pagamentos Multi-Gateway
// PROBLEMA: Uma plataforma de e-commerce precisa integrar com múltiplos gateways de pagamento
// (PagSeguro, MercadoPago, Stripe) e cada gateway tem componentes específicos (Processador, Validador, Logger)
// O código atual está muito acoplado e dificulta a adição de novos gateways

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic.FileIO;
using System;
using System.ComponentModel.DataAnnotations;

namespace DesignPatternChallenge
{
    // Contexto: Sistema de pagamentos que precisa trabalhar com diferentes gateways
    // Cada gateway tem sua própria forma de processar, validar e logar transações

    public class PaymentService : IPaymentService
    {
        private readonly IValidator _validator;
        private readonly IProcessTransaction _processor;
        private readonly ILogger _logger;

        public PaymentService(IPaymentGatewayFactory factory)
        {
            _validator = factory.CreateValidator();
            _processor = factory.CreateProcessor();
            _logger = factory.CreateLogger();
        }

        public void ProcessPayment(decimal amount, string cardNumber)
        {
            if (!_validator.ValidateCard(cardNumber))
            {
                Console.WriteLine("Cartão inválido");
                return;
            }

            var result = _processor.ProcessTransaction(amount, cardNumber);
            _logger.Log($"Transação processada: {result}");
        }
    }

    public interface IPaymentService
    {
        void ProcessPayment(decimal amount, string cardNumber);
    }

    // Componentes do PagSeguro
    public class PagSeguroValidator : IValidator
    {
        public bool ValidateCard(string cardNumber)
        {
            Console.WriteLine("PagSeguro: Validando cartão...");
            return cardNumber.Length == 16;
        }
    }

    public class PagSeguroProcessor : IProcessTransaction
    {
        public string ProcessTransaction(decimal amount, string cardNumber)
        {
            Console.WriteLine($"PagSeguro: Processando R$ {amount}...");
            return $"PAGSEG-{Guid.NewGuid().ToString().Substring(0, 8)}";
        }
    }

    public class PagSeguroLogger : ILogger
    {
        public void Log(string message)
        {
            Console.WriteLine($"[PagSeguro Log] {DateTime.Now}: {message}");
        }
    }
    public class PagSeguroFactory : IPaymentGatewayFactory
    {
        public IValidator CreateValidator() => new PagSeguroValidator();
        public IProcessTransaction CreateProcessor() => new PagSeguroProcessor();
        public ILogger CreateLogger() => new PagSeguroLogger();
    }

    // Componentes do MercadoPago
    public class MercadoPagoValidator : IValidator
    {
        public bool ValidateCard(string cardNumber)
        {
            Console.WriteLine("MercadoPago: Validando cartão...");
            return cardNumber.Length == 16 && cardNumber.StartsWith("5");
        }
    }

    public class MercadoPagoProcessor : IProcessTransaction
    {
        public string ProcessTransaction(decimal amount, string cardNumber)
        {
            Console.WriteLine($"MercadoPago: Processando R$ {amount}...");
            return $"MP-{Guid.NewGuid().ToString().Substring(0, 8)}";
        }
    }

    public class MercadoPagoLogger : ILogger
    {
        public void Log(string message)
        {
            Console.WriteLine($"[MercadoPago Log] {DateTime.Now}: {message}");
        }
    }

    public class MercadoPagoFactory : IPaymentGatewayFactory
    {
        public IValidator CreateValidator() => new MercadoPagoValidator();
        public IProcessTransaction CreateProcessor() => new MercadoPagoProcessor();
        public ILogger CreateLogger() => new MercadoPagoLogger();
    }

    // Componentes do Stripe
    public class StripeValidator : IValidator
    {
        public bool ValidateCard(string cardNumber)
        {
            Console.WriteLine("Stripe: Validando cartão...");
            return cardNumber.Length == 16 && cardNumber.StartsWith("4");
        }
    }

    public class StripeProcessor : IProcessTransaction
    {
        public string ProcessTransaction(decimal amount, string cardNumber)
        {
            Console.WriteLine($"Stripe: Processando ${amount}...");
            return $"STRIPE-{Guid.NewGuid().ToString().Substring(0, 8)}";
        }
    }

    public class StripeLogger : ILogger
    {
        public void Log(string message)
        {
            Console.WriteLine($"[Stripe Log] {DateTime.Now}: {message}");
        }
    }

    public class StripeFactory : IPaymentGatewayFactory
    {
        public IValidator CreateValidator() => new StripeValidator();
        public IProcessTransaction CreateProcessor() => new StripeProcessor();
        public ILogger CreateLogger() => new StripeLogger();
    }

    public interface IValidator
    {
        bool ValidateCard(string cardNumber);
    }

    public interface IProcessTransaction
    {
        string ProcessTransaction(decimal amount, string cardNumber);
    }

    public interface ILogger
    {
        void Log(string message);
    }
    public interface IPaymentGatewayFactory
    {
        IValidator CreateValidator();
        IProcessTransaction CreateProcessor();
        ILogger CreateLogger();
    }

    class Program
    {
        static void Main(string[] args)
        {
            var serviceProvider = new ServiceCollection()
                .AddScoped<PagSeguroFactory>()
                .AddScoped<MercadoPagoFactory>()
                .AddScoped<StripeFactory>()
                .BuildServiceProvider();

            Console.WriteLine("=== Sistema de Pagamentos ===\n");

            Console.WriteLine("Escolha gateway: 1-PagSeguro | 2-MercadoPago | 3-Stripe");
            var option = Console.ReadLine();

            IPaymentGatewayFactory factory = option switch
            {
                "1" => serviceProvider.GetRequiredService<PagSeguroFactory>(),
                "2" => serviceProvider.GetRequiredService<MercadoPagoFactory>(),
                "3" => serviceProvider.GetRequiredService<StripeFactory>(),
                _ => throw new Exception("Gateway inválido")
            };

            var paymentService = new PaymentService(factory);

            Console.WriteLine();
            paymentService.ProcessPayment(150m, "5234567890123456");

            Console.ReadLine();

            // Pergunta para reflexão:
            // - Como adicionar um novo gateway sem modificar PaymentService?
            // - Como garantir que todos os componentes de um gateway sejam compatíveis entre si?
            // - Como evitar criar componentes de gateways diferentes acidentalmente?
        }
    }
}
