using System.Runtime.Serialization;
using System.ServiceModel;
using System.Threading.Tasks;

namespace CaixaSeguradora.Api.SoapServices
{
    /// <summary>
    /// SOAP Service Contract for Solicitacao (Request) operations
    /// Namespace matches legacy Visual Age service for compatibility
    /// </summary>
    [ServiceContract(Namespace = "http://ls.caixaseguradora.com.br/LS1134WSV0002_Solicitacao/v1")]
    public interface ISolicitacaoService
    {
        /// <summary>
        /// Creates a new payment authorization request (Solicitacao)
        /// Maps to PaymentAuthorizationService in the backend
        /// </summary>
        /// <param name="request">Payment authorization details</param>
        /// <returns>Solicitacao response with authorization details</returns>
        [OperationContract]
        Task<SolicitacaoResponse> CriarSolicitacaoAsync(SolicitacaoRequest request);

        /// <summary>
        /// Queries an existing claim by protocol or claim number
        /// Maps to ClaimService.SearchClaimAsync in the backend
        /// </summary>
        /// <param name="request">Search criteria (protocol or claim number)</param>
        /// <returns>Claim details response</returns>
        [OperationContract]
        Task<ConsultaSolicitacaoResponse> ConsultarSolicitacaoAsync(ConsultaSolicitacaoRequest request);
    }

    #region Request/Response DTOs

    /// <summary>
    /// Request for creating a payment authorization
    /// </summary>
    [DataContract(Namespace = "http://ls.caixaseguradora.com.br/LS1134WSV0002_Solicitacao/v1")]
    public class SolicitacaoRequest
    {
        [DataMember(Order = 1)]
        public int Tipseg { get; set; }

        [DataMember(Order = 2)]
        public int Orgsin { get; set; }

        [DataMember(Order = 3)]
        public int Rmosin { get; set; }

        [DataMember(Order = 4)]
        public int Numsin { get; set; }

        [DataMember(Order = 5)]
        public int TipoPagamento { get; set; }

        [DataMember(Order = 6)]
        public decimal ValorPrincipal { get; set; }

        [DataMember(Order = 7)]
        public decimal ValorCorrecao { get; set; }

        [DataMember(Order = 8)]
        public string? Favorecido { get; set; }

        [DataMember(Order = 9)]
        public string TipoApolice { get; set; } = "1";

        [DataMember(Order = 10)]
        public string? Observacoes { get; set; }

        [DataMember(Order = 11)]
        public string CodUsuario { get; set; } = "SOAP_USER";
    }

    /// <summary>
    /// Response from payment authorization creation
    /// </summary>
    [DataContract(Namespace = "http://ls.caixaseguradora.com.br/LS1134WSV0002_Solicitacao/v1")]
    public class SolicitacaoResponse
    {
        [DataMember(Order = 1)]
        public bool Sucesso { get; set; }

        [DataMember(Order = 2)]
        public int Ocorhist { get; set; }

        [DataMember(Order = 3)]
        public int Operacao { get; set; }

        [DataMember(Order = 4)]
        public string DataMovimento { get; set; } = string.Empty;

        [DataMember(Order = 5)]
        public string HoraOperacao { get; set; } = string.Empty;

        [DataMember(Order = 6)]
        public decimal ValorPrincipalBTNF { get; set; }

        [DataMember(Order = 7)]
        public decimal ValorCorrecaoBTNF { get; set; }

        [DataMember(Order = 8)]
        public decimal ValorTotalBTNF { get; set; }

        [DataMember(Order = 9)]
        public decimal ValorPendenteAtualizado { get; set; }

        [DataMember(Order = 10)]
        public string? MensagemErro { get; set; }

        [DataMember(Order = 11)]
        public string? CodigoErro { get; set; }
    }

    /// <summary>
    /// Request for querying claim details
    /// </summary>
    [DataContract(Namespace = "http://ls.caixaseguradora.com.br/LS1134WSV0002_Solicitacao/v1")]
    public class ConsultaSolicitacaoRequest
    {
        [DataMember(Order = 1)]
        public int? Fonte { get; set; }

        [DataMember(Order = 2)]
        public int? Protsini { get; set; }

        [DataMember(Order = 3)]
        public int? Dac { get; set; }

        [DataMember(Order = 4)]
        public int? Orgsin { get; set; }

        [DataMember(Order = 5)]
        public int? Rmosin { get; set; }

        [DataMember(Order = 6)]
        public int? Numsin { get; set; }
    }

    /// <summary>
    /// Response with claim details
    /// </summary>
    [DataContract(Namespace = "http://ls.caixaseguradora.com.br/LS1134WSV0002_Solicitacao/v1")]
    public class ConsultaSolicitacaoResponse
    {
        [DataMember(Order = 1)]
        public bool Sucesso { get; set; }

        [DataMember(Order = 2)]
        public string? NumeroProtocolo { get; set; }

        [DataMember(Order = 3)]
        public string? NumeroSinistro { get; set; }

        [DataMember(Order = 4)]
        public string? NumeroApolice { get; set; }

        [DataMember(Order = 5)]
        public decimal ValorPendente { get; set; }

        [DataMember(Order = 6)]
        public decimal SaldoPago { get; set; }

        [DataMember(Order = 7)]
        public decimal TotalPago { get; set; }

        [DataMember(Order = 8)]
        public bool EhConsorcio { get; set; }

        [DataMember(Order = 9)]
        public string? NomeRamo { get; set; }

        [DataMember(Order = 10)]
        public string? NomeSeguradora { get; set; }

        [DataMember(Order = 11)]
        public string? MensagemErro { get; set; }

        [DataMember(Order = 12)]
        public string? CodigoErro { get; set; }
    }

    #endregion
}
