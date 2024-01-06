using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HighwayToPeak.Models
{
    public class OxygenClimber : Climber
    {
        private const int InitialStamina = 10;
        private const int StaminaRecoveryRate = 1;

        public OxygenClimber(string name)
            : base(name, InitialStamina)
        {
        }

        public override void Rest(int daysCount)
        {
            this.Stamina += daysCount * StaminaRecoveryRate;
        }
    }
}
