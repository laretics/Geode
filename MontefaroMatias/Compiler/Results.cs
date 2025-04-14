using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MontefaroMatias.Compiler
{
    //Objeto que devuelve como argumento una llamada a una función
    public abstract class commandResult
    {
        public override string ToString() { return string.Empty; }
    }
    public class sucessResult : commandResult { }
    public class warningResult : commandResult
    {
        public string message { get; private set; }
        public int lineCode { get; private set; }
        public virtual string keyWord { get => "WARNING"; }
        public int errorId { get; private set; }
        public warningResult(int id, string message, int lineCode) : base()
        {
            errorId = id;
            this.message = message;
            this.lineCode = lineCode;
        }
        public warningResult(int id, string message) : this(id, message, -1) { }
        public override string ToString()
        {
            if (lineCode < 0)
                return string.Format("{0} {1}: {2}.", keyWord, errorId, message);
            else
                return string.Format("{0} {1} in line {2}: {3}.", keyWord, errorId, lineCode, message);
        }
    }
    public class errorResult : warningResult
    {
        public errorResult(int id, string message, int lineCode) : base(id, message, lineCode) { }
        public override string keyWord { get => "ERROR"; }
    }
}
