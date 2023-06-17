using LibBee.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibBee
{
    public class ActionRecorder
    {
        private static readonly ActionRecorder instance = new ();

        public bool IsRecording
        {
            get;
            private set;
        }

        public List<ActionCommand> RecordedActions
        {
            get;
            private set;
        }

        private ActionRecorder() 
        {
            IsRecording = false;
            RecordedActions = new();
        }

        public static ActionRecorder GetInstance()
        {
            return instance;
        }

        public void Start()
        {
            IsRecording = true;
            RecordedActions.Clear();
        }

        public void End()
        {
            IsRecording = false;
        }

        internal void AddCommand(ActionCommand command)
        {
            RecordedActions.Add(command);
        }
    }
}
