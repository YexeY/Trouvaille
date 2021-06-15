using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Trouvaille_WebAPI.Globals
{
    public class Globals
    {
        public enum PaymentMethod 
        { 
            Rechnung,
            Vorkasse,
            Paypal
        }

        public enum Shipmentmethod
        {
            dhl,
            dpd,
            ups,
            hermes
        }

        public enum OrderState
        {
            Bestellt,
            Unterwegs,
            Zugestellt,
            Storniert,
        }

        public enum GermanStates
        {
            BadenWürttemberg,
            Bayer, 
            Berlin,
            Brandenburg,
            Bremen,
            Hamburg, 
            Hessen, 
            Mecklenburg_Vorpommern,
            Niedersachen,
            Nordhein_Westfalen,
            RheinlandPfalz,
            Saarland,
            Sachsen,
            Sachsen_Anhalt, 
            SchleswigHolstein,
            Thüringen
        }
    }
}
