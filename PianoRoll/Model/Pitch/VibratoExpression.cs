using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PianoRoll.Model.Pitch
{

    public class VibratoExpression : UExpression
    {
        public VibratoExpression() : base("vibrato", "VBR")
        {
        }

        private double _length;
        private double _period;
        private double _depth;
        private double _in;
        private double _out;
        private double _shift;
        private double _drift;

        public double Length
        {
            set => _length = Math.Max(0, Math.Min(100, value));
            get => _length;
        }

        public double Period
        {
            set => _period = Math.Max(64, Math.Min(512, value));
            get => _period;
        }

        public double Depth
        {
            set => _depth = Math.Max(5, Math.Min(200, value));
            get => _depth;
        }

        public double In
        {
            set
            {
                _in = Math.Max(0, Math.Min(100, value));
                _out = Math.Min(_out, 100 - value);
            }
            get => _in;
        }

        public double Out
        {
            set
            {
                _out = Math.Max(0, Math.Min(100, value));
                _in = Math.Min(_in, 100 - value);
            }
            get => _out;
        }

        public double Shift
        {
            set => _shift = Math.Max(0, Math.Min(100, value));
            get => _shift;
        }

        public double Drift
        {
            set => _drift = Math.Max(-100, Math.Min(100, value));
            get => _drift;
        }

        public override string Type => "pitch";

        public override object Data { set; get; }
    }
}
