using System;

namespace XRXP.Recorder.Models
{
    public class QuestionProperty : RecordBase
    {
        public string Property;
        public string Value;
        public string QuestionId;

        private Question _question;

        public QuestionProperty(string property, string value, Question question, string protocol = "")
        {
            this.Id = Guid.NewGuid().ToString();
            this.Property = property;
            this.Value = value;
            this._question = question;
            this.QuestionId = question.Id;
            this.Protocol = protocol.Length > 0 ? protocol : "QuestionProperty";
        }

        public void UpdateValue(string value)
        {
            this.Value = value;
        }
    }
}
