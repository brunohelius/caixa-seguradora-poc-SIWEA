// T135 [UI Polish] - Centralized Error Messages Resource File
// This is the auto-generated designer file for the .resx file

namespace CaixaSeguradora.Core.Resources
{
    using System;
    using System.Resources;

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class ErrorMessages {

        private static System.Resources.ResourceManager resourceMan;

        private static System.Globalization.CultureInfo resourceCulture;

        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal ErrorMessages() {
        }

        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    System.Resources.ResourceManager temp = new System.Resources.ResourceManager("CaixaSeguradora.Core.Resources.ErrorMessages", typeof(ErrorMessages).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }

        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }

        // Error message properties
        internal static string ClaimNotFoundProtocol {
            get {
                return ResourceManager.GetString("ClaimNotFoundProtocol", resourceCulture);
            }
        }

        internal static string ClaimNotFoundNumber {
            get {
                return ResourceManager.GetString("ClaimNotFoundNumber", resourceCulture);
            }
        }

        internal static string PaymentTypeInvalid {
            get {
                return ResourceManager.GetString("PaymentTypeInvalid", resourceCulture);
            }
        }

        internal static string BeneficiaryRequired {
            get {
                return ResourceManager.GetString("BeneficiaryRequired", resourceCulture);
            }
        }

        internal static string InsufficientBalance {
            get {
                return ResourceManager.GetString("InsufficientBalance", resourceCulture);
            }
        }

        internal static string ExternalValidationFailed {
            get {
                return ResourceManager.GetString("ExternalValidationFailed", resourceCulture);
            }
        }

        internal static string CurrencyRateNotFound {
            get {
                return ResourceManager.GetString("CurrencyRateNotFound", resourceCulture);
            }
        }
    }
}
