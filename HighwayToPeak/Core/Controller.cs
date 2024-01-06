using HighwayToPeak.Core.Contracts;
using HighwayToPeak.Models.Contracts;
using HighwayToPeak.Models;
using HighwayToPeak.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HighwayToPeak.Utilities.Messages;
using System.Reflection.Metadata;

namespace HighwayToPeak.Core
{
    public class Controller : IController
    {
        private readonly PeakRepository peaks;
        private readonly ClimberRepository climbers;
        private readonly BaseCamp baseCamp;

        public Controller()
        {
            peaks = new PeakRepository();
            climbers = new ClimberRepository();
            baseCamp = new BaseCamp();
        }

        public string AddPeak(string name, int elevation, string difficultyLevel)
        {
            if (peaks.Get(name) != null)
            {
                return string.Format(OutputMessages.PeakAlreadyAdded,name);
            }

            if (difficultyLevel != "Extreme" && difficultyLevel != "Hard" && difficultyLevel != "Moderate")
            {
                return string.Format(OutputMessages.PeakDiffucultyLevelInvalid, difficultyLevel);
            }

            IPeak peak = new Peak(name, elevation, difficultyLevel);
            peaks.Add(peak);

            return string.Format(OutputMessages.PeakIsAllowed, name, "PeakRepository");
        }

        public string NewClimberAtCamp(string name, bool isOxygenUsed)
        {
            if (climbers.Get(name) != null)
            {
                return $"{name} is a participant in {climbers.GetType().Name} and cannot be duplicated.";
                return string.Format(OutputMessages.ClimberCannotBeDuplicated, name, climbers.GetType().Name);
            }

            IClimber climber = isOxygenUsed ? new OxygenClimber(name) : new NaturalClimber(name);
            climbers.Add(climber);
            baseCamp.ArriveAtCamp(name);

            return $"{name} has arrived at the BaseCamp and will wait for the best conditions.";
        }

        public string AttackPeak(string climberName, string peakName)
        {
            IClimber climber = climbers.Get(climberName);
            if (climber == null)
            {
                return $"Climber - {climberName}, has not arrived at the BaseCamp yet.";
            }

            IPeak peak = peaks.Get(peakName);
            if (peak == null)
            {
                return $"{peakName} is not allowed for international climbing.";
            }

            if (!baseCamp.Residents.Contains(climberName))
            {
                return $"{climberName} not found for gearing and instructions. The attack of {peakName} will be postponed.";
            }

            if (peak.DifficultyLevel == "Extreme" && climber.GetType() == typeof(NaturalClimber))
            {
                return $"{climberName} does not cover the requirements for climbing {peakName}.";
            }

            baseCamp.LeaveCamp(climberName);
            climber.Climb(peak);

            if (climber.Stamina == 0)
            {
                return $"{climberName} did not return to BaseCamp.";
            }

            baseCamp.ArriveAtCamp(climberName);
            return $"{climberName} successfully conquered {peakName} and returned to BaseCamp.";
        }

        public string CampRecovery(string climberName, int daysToRecover)
        {
            IClimber climber = climbers.Get(climberName);
            if (climber == null || !baseCamp.Residents.Contains(climberName))
            {
                return $"{climberName} not found at the BaseCamp.";
            }

            if (climber.Stamina == 10)
            {
                return $"{climberName} has no need of recovery.";
            }

            climber.Rest(daysToRecover);
            return $"{climberName} has been recovering for {daysToRecover} days and is ready to attack the mountain.";
        }

        public string BaseCampReport()
        {
            if (baseCamp.Residents.Count == 0)
            {
                return "BaseCamp is currently empty.";
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("BaseCamp residents:");
            foreach (string resident in baseCamp.Residents)
            {
                IClimber climber = climbers.Get(resident);
                sb.AppendLine($"Name: {climber.Name}, Stamina: {climber.Stamina}, Count of Conquered Peaks: {climber.ConqueredPeaks.Count}");
            }

            return sb.ToString().TrimEnd();
        }

        public string OverallStatistics()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("***Highway-To-Peak***");

            List<IClimber> sortedClimbers = climbers.All.OrderByDescending(c => c.ConqueredPeaks.Count)
                .ThenBy(c => c.Name).ToList();

            foreach (IClimber climber in sortedClimbers)
            {
                sb.AppendLine($"{climber}");

                List<IPeak> conqueredPeaks = climber.ConqueredPeaks.Select(p => peaks.Get(p)).OrderByDescending(p => p.Elevation).ToList();
                foreach (IPeak peak in conqueredPeaks)
                {
                    sb.AppendLine($"{peak}");
                }
            }

            return sb.ToString().TrimEnd();
        }
    }
}
