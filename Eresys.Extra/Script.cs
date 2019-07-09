using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Eresys.Extra
{
    public class ScriptValue
    {
        public string Name { get; set; }

        public string Value { get; set; }
    }

    public class ScriptBlock
    {
        public string this[string i] => _values.First(p => p.Name == i).Value;

        public string this[int i] => _values[i].Value;

        public int Count => _values.Count;

        public ScriptBlock(string script)
        {
            _values = new List<ScriptValue>();
            _script = script;
            _cursor = 0;

            while (_cursor < script.Length)
            {
                try
                {
                    var val = new ScriptValue()
                    {
                        Name = ReadWord(),
                        Value = ReadWord(),
                    };

                    _values.Add(val);
                }
                catch (Exception)
                {
                    break;
                }
            }
        }

        private string ReadWord()
        {
            var sb = new StringBuilder();

            while (_script[_cursor] != '"') _cursor++;

            _cursor++;

            while (_script[_cursor] != '"')
            {
                sb.Append(_script[_cursor]);
                _cursor++;
            }

            _cursor++;

            return sb.ToString();
        }

        private int _cursor;
        private readonly string _script;
        private readonly List<ScriptValue> _values;
    }

    public class Script
    {
        public ScriptBlock[] this[string i]
        {
            get
            {
                var classes = i.Split(';');
                var queue = new Queue();
                for (int j = 0; j < _blocks.Count; j++)
                {
                    for (int k = 0; k < classes.Length; k++)
                    {
                        if (_blocks[j]["classname"] == classes[k])
                        {
                            queue.Enqueue(_blocks[j]);
                        }
                    }
                }

                var res = new ScriptBlock[queue.Count];

                for (int j = 0; j < res.Length; j++)
                {
                    res[j] = (ScriptBlock)queue.Dequeue();
                }

                return res;
            }
        }

        public ScriptBlock this[int i] => _blocks[i];

        public int Count => _blocks.Count;

        public Script(string script)
        {
            int i = 0;
            _blocks = new List<ScriptBlock>();
            while (i < script.Length)
            {
                try
                {
                    while (script[i] != '{') i++;
                    i++;
                    int start = i;
                    while (script[i] != '}') i++;
                    int stop = i - 1;
                    i++;
                    _blocks.Add(new ScriptBlock(script.Substring(start, stop - start + 1)));
                }
                catch (Exception)
                {
                    break;
                }
            }
        }

        private readonly List<ScriptBlock> _blocks;
    }

    public static class ScriptExtensions
    {
        public static float WorldspawnMaxRange(this Script script)
        {
            try
            {
                return int.Parse(script["worldspawn"][0]["MaxRange"]);
            }
            catch (Exception)
            {
                return 0x4000;
            }
        }

        public static string WorldspawnSkyName(this Script script)
        {
            try
            {
                return script["worldspawn"][0]["skyname"] + ".bmp";
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}
