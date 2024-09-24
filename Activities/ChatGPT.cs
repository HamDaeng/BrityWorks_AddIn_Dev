using BrityWorks.AddIn.Hyper.Properties;
using RPAGO.AddIn;
using RPAGO.Common.Data;
using RPAGO.Common.Library;
using System.Collections.Generic;
using Bitmap = System.Drawing.Bitmap;
using Microsoft.ClearScript.V8;
using Microsoft.ClearScript;
using PuppeteerSharp;

namespace BrityWorks.AddIn.Hyper.Activities
{
    public class ChatGPT : IActivityItem
    {
        public static readonly PropKey OutputPropKey = new PropKey("GPT", "Prop1");

        public static readonly PropKey InputPropKey_K = new PropKey("GPT", "Prop2");
        public static readonly PropKey InputPropKey_Q = new PropKey("GPT", "Prop3");


        public string DisplayName => "GhatGPT";

        public Bitmap Icon => Resources.excute;

        public LibraryHeadlessType Mode => LibraryHeadlessType.Both;

        public PropKey DisplayTextProperty => OutputPropKey;

        public PropKey OutputProperty => OutputPropKey;

        private PropertySet PropertyList;

        public List<Property> OnCreateProperties()
        {
            var properties = new List<Property>()
            {
                new Property(this, OutputPropKey, "답변").SetRequired(),
                new Property(this, InputPropKey_K, "API Key").SetRequired(),
                new Property(this, InputPropKey_Q, "질문").SetRequired(),
            };

            return properties;
        }

        public void OnLoad(PropertySet properties)
        {
            PropertyList = properties;
        }


        public object OnRun(IDictionary<string, object> properties)
        {
            // 클리어 스크립트 선언 ( 형변환 전용 )
            V8ScriptEngine v8 = new V8ScriptEngine();

            var obj = properties;

            // obj를 이용하여 모든 값들을 빼옴
            v8.AddHostObject("obj", HostItemFlags.GlobalMembers, obj);

            v8.Execute("var api_key = obj.GPT_Prop2");
            v8.Execute("var question = obj.GPT_Prop3");

            // 값들을 알맞은 형태로 변환하여 새로운 변수에 선언
            string api_key = (string)(v8.Evaluate("path"));
            string question = (string)(v8.Evaluate("name"));
            


            return api_key;
        }
    }
}
