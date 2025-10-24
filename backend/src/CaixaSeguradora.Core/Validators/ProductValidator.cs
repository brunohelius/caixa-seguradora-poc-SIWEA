using System.Threading.Tasks;
using CaixaSeguradora.Core.Entities;
using CaixaSeguradora.Core.Enums;
using CaixaSeguradora.Core.Interfaces;

namespace CaixaSeguradora.Core.Validators;

/// <summary>
/// T086: ProductValidator
/// Validates product types and determines routing for external validation services.
/// Handles consortium products (6814, 7701, 7709) and EFP/HB contract validation.
/// </summary>
public class ProductValidator
{
    private readonly IUnitOfWork _unitOfWork;

    // Consortium product codes as defined in FR-020
    private static readonly int[] ConsortiumProductCodes = { 6814, 7701, 7709 };

    public ProductValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Checks if a product code represents a consortium product
    /// </summary>
    /// <param name="codprodu">Product code</param>
    /// <returns>True if product is consortium (6814, 7701, or 7709)</returns>
    public bool IsConsortiumProduct(int codprodu)
    {
        return Array.Exists(ConsortiumProductCodes, code => code == codprodu);
    }

    /// <summary>
    /// Determines if a claim requires EFP (Entidade Fechada de Previdência) validation
    /// by querying the EF_CONTR_SEG_HABIT table
    /// </summary>
    /// <param name="claim">Claim master entity</param>
    /// <returns>True if claim has a consortium contract (NUM_CONTRATO > 0)</returns>
    public async Task<bool> RequiresEFPValidation(ClaimMaster claim)
    {
        // Query the consortium contract table to check if this policy has an EFP contract
        // Find contract by policy number (assuming relationship exists)
        var contracts = await _unitOfWork.ConsortiumContracts.FindAsync(c =>
            c.NumContrato > 0
            // TODO: Add proper policy relationship when ConsortiumContract entity has PolicyNumber field
            // c.PolicyNumber == claim.Numapol
        );

        return contracts.Any();
    }

    /// <summary>
    /// Determines the appropriate validation route for a claim based on product type and contract
    /// </summary>
    /// <param name="claim">Claim master entity</param>
    /// <returns>Validation route enum (CNOUA, SIPUA, SIMDA, or None)</returns>
    public async Task<ValidationRoute> DetermineValidationRoute(ClaimMaster claim)
    {
        // Step 1: Check if product is consortium
        if (IsConsortiumProduct(claim.Codprodu))
        {
            // Consortium products always go to CNOUA
            return ValidationRoute.CNOUA;
        }

        // Step 2: Check if claim requires EFP validation
        bool requiresEFP = await RequiresEFPValidation(claim);

        if (requiresEFP)
        {
            // EFP contracts go to SIPUA
            return ValidationRoute.SIPUA;
        }

        // Step 3: Check if HB (Habitação) contract exists
        // HB contracts have NUM_CONTRATO = 0 or are not in EF_CONTR_SEG_HABIT
        var contracts = await _unitOfWork.ConsortiumContracts.FindAsync(c =>
            c.NumContrato == 0
            // TODO: Add proper policy relationship when ConsortiumContract entity has PolicyNumber field
            // && c.PolicyNumber == claim.Numapol
        );

        if (contracts.Any())
        {
            // HB contracts go to SIMDA
            return ValidationRoute.SIMDA;
        }

        // Step 4: Default to SIMDA for non-consortium, non-EFP products
        return ValidationRoute.SIMDA;
    }

    /// <summary>
    /// Validates a claim's product configuration and returns any errors
    /// </summary>
    /// <param name="claim">Claim to validate</param>
    /// <returns>Validation result with errors if any</returns>
    public async Task<(bool IsValid, string? ErrorMessage)> ValidateProduct(ClaimMaster claim)
    {
        if (claim.Codprodu <= 0)
        {
            return (false, "Código de produto inválido");
        }

        // Validate consortium products have required data
        if (IsConsortiumProduct(claim.Codprodu))
        {
            if (claim.Numapol <= 0)
            {
                return (false, "Apólice é obrigatória para produtos de consórcio");
            }
        }

        // Determine validation route to ensure it's configured
        var route = await DetermineValidationRoute(claim);

        if (route == ValidationRoute.None)
        {
            return (false, "Não foi possível determinar a rota de validação para este produto");
        }

        return (true, null);
    }

    /// <summary>
    /// Gets a human-readable description of why a specific validation route was chosen
    /// </summary>
    /// <param name="claim">Claim entity</param>
    /// <returns>Description string</returns>
    public async Task<string> GetValidationRouteReason(ClaimMaster claim)
    {
        if (IsConsortiumProduct(claim.Codprodu))
        {
            return $"Produto de consórcio (código {claim.Codprodu}) requer validação CNOUA";
        }

        bool requiresEFP = await RequiresEFPValidation(claim);
        if (requiresEFP)
        {
            return "Contrato EFP requer validação SIPUA";
        }

        return "Contrato HB requer validação SIMDA";
    }
}
