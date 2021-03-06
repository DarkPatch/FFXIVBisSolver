﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OPTANO.Modeling.Optimization;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace FFXIVBisSolver
{
    public class PiecewiseLinearFunction
    {

        public PiecewiseLinearFunction() : this(new List<Tuple<int, int>>())
        {
        }

        public PiecewiseLinearFunction(IEnumerable<Tuple<int, int>> segments)
        {
            Segments = segments.ToList();
        }

        public List<Tuple<int, int>> Segments { get; set; }

        // returns the final SOS2 factor, for checking whether the piecewise's function bound was sufficient
        public Variable AddToModel(Model model, Variable x, Variable y, bool useSosWorkaround)
        {
            var range = Enumerable.Range(0, Segments.Count);
            var factor = new VariableCollection<int>(model, range, debugNameGenerator: i => new StringBuilder().AppendFormat("sos2_factor_{0}", i));
            var sos2Vars = Segments.ToDictionary(kv => factor[Segments.IndexOf(kv)],
                kv => (double) Segments.IndexOf(kv));

            model.AddSOS2(sos2Vars, useSosWorkaround);
            model.AddConstraint(Expression.Sum(sos2Vars.Keys) == 1);
            model.AddConstraint(x == Expression.Sum(Segments.Select((kv, ix) => factor[ix] * kv.Item1)));

            model.AddConstraint(y == Expression.Sum(Segments.Select((kv, ix) => factor[ix] * kv.Item2)));

            return factor[Segments.Count - 1];
        }
    }
}
