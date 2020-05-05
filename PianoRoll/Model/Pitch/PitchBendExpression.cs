using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PianoRoll.Model.Pitch
{
    public class PitchBendExpression : UExpression
    {
        public PitchBendExpression() : base("pitch", "PIT")
        {
        }

        protected List<PitchPoint> _data = new List<PitchPoint>();
        protected bool _snapFirst = true;

        public override string Type => "pitch";

        public override object Data
        {
            set => _data = (List<PitchPoint>)value;
            get => _data;
        }

        public List<PitchPoint> Points => _data;

        public bool SnapFirst
        {
            set => _snapFirst = value;
            get => _snapFirst;
        }

        public void AddPoint(PitchPoint p)
        {
            _data.Add(p);
            _data.Sort();
        }

        public void RemovePoint(PitchPoint p)
        {
            _data.Remove(p);
        }

        public int[] Array { get; set; }
    }
}
