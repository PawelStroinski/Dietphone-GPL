using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Linq;

namespace Dietphone.Models
{
    public class Insulin : EntityWithId
    {
        public DateTime DateTime { get; set; }
        public string Note { get; set; }
        public float NormalBolus { get; set; }
        public float SquareWaveBolus { get; set; }
        public float SquareWaveBolusHours { get; set; }
        protected List<Guid> circumstances;

        [XmlIgnore]
        public IEnumerable<InsulinCircumstance> Circumstances
        {
            get
            {
                CheckCircumstances();
                foreach (var id in circumstances)
                {
                    var circumstance = Finder.FindInsulinCircumstanceById(id);
                    yield return circumstance != null ? circumstance : DefaultEntities.InsulinCircumstance;
                }
            }
        }

        public void InitializeCircumstances(List<Guid> newCircumstances)
        {
            var alreadyInitialized = circumstances != null;
            if (alreadyInitialized)
            {
                throw new InvalidOperationException("Circumstances can only be initialized once.");
            }
            circumstances = newCircumstances;
        }

        public IEnumerable<Guid> ReadCircumstances()
        {
            return circumstances.ToList();
        }

        public void AddCircumstance(InsulinCircumstance newCircumstance)
        {
            CheckCircumstances();
            circumstances.Add(newCircumstance.Id);
        }

        public void RemoveCircumstance(InsulinCircumstance circumstanceToDelete)
        {
            CheckCircumstances();
            circumstances.Remove(circumstanceToDelete.Id);
        }

        private void CheckCircumstances()
        {
            if (circumstances == null)
            {
                throw new InvalidOperationException("Call InitializeCircumstances first.");
            }
        }
    }
}
