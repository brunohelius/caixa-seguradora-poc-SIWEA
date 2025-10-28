using CaixaSeguradora.Core.DTOs;
using CaixaSeguradora.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.ServiceModel;
using System.Threading.Tasks;

namespace CaixaSeguradora.Api.SoapServices
{
    /// <summary>
    /// SOAP Service implementation for Solicitacao operations
    /// Provides legacy compatibility with Visual Age SOAP interfaces
    /// </summary>
    public class SolicitacaoService : ISolicitacaoService
    {
        private readonly IPaymentAuthorizationService _paymentService;
        private readonly IClaimService _claimService;
        private readonly ILogger<SolicitacaoService> _logger;

        public SolicitacaoService(
            IPaymentAuthorizationService paymentService,
            IClaimService claimService,
            ILogger<SolicitacaoService> logger)
        {
            _paymentService = paymentService;
            _claimService = claimService;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new payment authorization request via SOAP
        /// </summary>
        public async Task<SolicitacaoResponse> CriarSolicitacaoAsync(SolicitacaoRequest request)
        {
            _logger.LogInformation(
                "SOAP CriarSolicitacao called for claim {Orgsin}/{Rmosin}/{Numsin} by user {User}",
                request.Orgsin, request.Rmosin, request.Numsin, request.CodUsuario);

            try
            {
                // Map SOAP request to internal DTO
                var paymentRequest = new PaymentAuthorizationRequest
                {
                    TipoPagamento = request.TipoPagamento,
                    ValorPrincipal = request.ValorPrincipal,
                    ValorCorrecao = request.ValorCorrecao,
                    Favorecido = request.Favorecido,
                    TipoApolice = request.TipoApolice,
                    Observacoes = request.Observacoes
                };

                // Call internal service
                var result = await _paymentService.AuthorizePaymentAsync(
                    request.Tipseg,
                    request.Orgsin,
                    request.Rmosin,
                    request.Numsin,
                    paymentRequest,
                    request.CodUsuario);

                // Map response
                return new SolicitacaoResponse
                {
                    Sucesso = true,
                    Ocorhist = result.Ocorhist,
                    Operacao = result.Operacao,
                    DataMovimento = result.DataMovimento.ToString("dd/MM/yyyy"),
                    HoraOperacao = result.HoraOperacao.ToString(@"hh\:mm\:ss"),
                    ValorPrincipalBTNF = result.ValorPrincipalBTNF,
                    ValorCorrecaoBTNF = result.ValorCorrecaoBTNF,
                    ValorTotalBTNF = result.ValorTotalBTNF,
                    ValorPendenteAtualizado = result.ValorPendenteAtualizado
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SOAP CriarSolicitacao failed for claim {Orgsin}/{Rmosin}/{Numsin}",
                    request.Orgsin, request.Rmosin, request.Numsin);

                // Return SOAP Fault with Portuguese error message
                var faultReason = new FaultReason($"Erro ao criar solicitação: {ex.Message}");
                var faultCode = new FaultCode("Server");

                // For validation errors, return structured response instead of fault
                if (ex is ArgumentException || ex.Message.Contains("validação") || ex.Message.Contains("validacao"))
                {
                    return new SolicitacaoResponse
                    {
                        Sucesso = false,
                        MensagemErro = ex.Message,
                        CodigoErro = "VALIDACAO_FALHOU"
                    };
                }

                // For other errors, throw SOAP fault
                throw new FaultException(faultReason, faultCode);
            }
        }

        /// <summary>
        /// Queries claim details via SOAP
        /// </summary>
        public async Task<ConsultaSolicitacaoResponse> ConsultarSolicitacaoAsync(ConsultaSolicitacaoRequest request)
        {
            _logger.LogInformation(
                "SOAP ConsultarSolicitacao called with Fonte={Fonte}, Protsini={Protsini}, Orgsin={Orgsin}",
                request.Fonte, request.Protsini, request.Orgsin);

            try
            {
                // Map SOAP request to internal search criteria
                var searchCriteria = new ClaimSearchCriteria
                {
                    Fonte = request.Fonte,
                    Protsini = request.Protsini,
                    Dac = request.Dac,
                    Orgsin = request.Orgsin,
                    Rmosin = request.Rmosin,
                    Numsin = request.Numsin
                };

                // Call internal service
                var claim = await _claimService.SearchClaimAsync(searchCriteria);

                if (claim == null)
                {
                    return new ConsultaSolicitacaoResponse
                    {
                        Sucesso = false,
                        MensagemErro = "DOCUMENTO NAO CADASTRADO",
                        CodigoErro = "SINISTRO_NAO_ENCONTRADO"
                    };
                }

                // Map response
                return new ConsultaSolicitacaoResponse
                {
                    Sucesso = true,
                    NumeroProtocolo = claim.NumeroProtocolo,
                    NumeroSinistro = claim.NumeroSinistro,
                    NumeroApolice = claim.NumeroApolice,
                    ValorPendente = claim.ValorPendente,
                    SaldoPago = claim.SaldoPago,
                    TotalPago = claim.TotalPago,
                    EhConsorcio = claim.EhConsorcio,
                    NomeRamo = claim.NomeRamo,
                    NomeSeguradora = claim.NomeSeguradora
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SOAP ConsultarSolicitacao failed");

                var faultReason = new FaultReason($"Erro ao consultar solicitação: {ex.Message}");
                var faultCode = new FaultCode("Server");
                throw new FaultException(faultReason, faultCode);
            }
        }
    }
}
