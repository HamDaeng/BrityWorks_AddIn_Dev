using BrityWorks.AddIn.Hyper.Properties;
using RPAGO.AddIn;
using RPAGO.Common.Data;
using RPAGO.Common.Library;
using System;
using System.Collections.Generic;
using Bitmap = System.Drawing.Bitmap;
using Microsoft.ClearScript.V8;
using Microsoft.ClearScript;
using PuppeteerSharp;
using RPAGO.Common.Event;

namespace BrityWorks.AddIn.Hyper.Activities
{
    public class CaptureChrome : IActivityItem
    {
        // 디자이너 옵션들 (Resources/String/ xaml 파일들에서 세부내용 수정)
        public static readonly PropKey OutputPropKey = new PropKey("Capture", "Prop1");
        public static readonly PropKey InputPropKey_Find = new PropKey("Capture", "Prop2");
        public static readonly PropKey InputPropKey_Path = new PropKey("Capture", "Prop3");
        public static readonly PropKey InputPropKey_Name = new PropKey("Capture", "Prop4");
        public static readonly PropKey FileExtension = new PropKey("Capture", "Prop5");

        // 카드 이름
        public string DisplayName => "Chrome Find Capture";

        // 아이콘 설정
        public Bitmap Icon => Resources.excute;

        public LibraryHeadlessType Mode => LibraryHeadlessType.Both;

        // 아웃풋 설정
        public PropKey DisplayTextProperty => OutputPropKey;
        public PropKey OutputProperty => OutputPropKey;

        // 아래에서 사용될 propertylist 선언
        private PropertySet PropertyList;

        public List<Property> OnCreateProperties()
        {
            var properties = new List<Property>()
            {
                // 카드의 옵션들에 대한 세부적인 설정 (내용, 들어가는 값 등)
                new Property(this, OutputPropKey, "RESULT").SetRequired(),
                new Property(this, InputPropKey_Find, "").SetRequired(),
                new Property(this, InputPropKey_Path, "").SetRequired(),
                new Property(this, InputPropKey_Name, "''").SetRequired(),
                new Property(this, FileExtension, "png").SetDropDownList("png;jpeg;").SetValueChangedHandler(OnTogglePropValueChanged),
            };

            return properties;
        }

        // 선택하는 옵션 전용
        void OnTogglePropValueChanged(object oldValue, object newValue)
        {
            var togglePropItem = PropertyList[FileExtension];

            // clearscript 이용하는 이유는, 자꾸 cast 과정에서 오류가 발생하여
            // 강제로 형변환을 하기 위해 이용
            V8ScriptEngine v8 = new V8ScriptEngine();

            v8.AddHostObject("obj", HostItemFlags.GlobalMembers, newValue);
            v8.Execute("var nv = newValue");

            var new_Value = (string)(v8.Evaluate("nv"));

            if ((string)new_Value == "png")
            {
                togglePropItem.SetVisible(true);
            }
            else if ((string)new_Value == "jpeg")
            {
                togglePropItem.SetVisible(false);
            }

            ReloadPropertyEvent.Publish();
        }

        // 로드 되었을 때
        public void OnLoad(PropertySet properties)
        {
            PropertyList = properties;
        }

        // 실행 시 (카드 run)
        public object OnRun(IDictionary<string, object> properties)
        {
            // 클리어스크립트 선언
            V8ScriptEngine v8 = new V8ScriptEngine();

            // 마지막 결과값 (output)
            bool success = true;

            try
            {
                // properties[FileExtension] 이렇게 사용시, cast가 어려워 클리어스크립트로 형변환을 하기 위해 properties 전체를 obj에다가 
                var obj = properties;
                // 아래서 사용될 옵션 (jpg,png 퀄리티... 등)
                ScreenshotOptions option = new ScreenshotOptions();

                // obj를 이용하여 모든 값들을 빼옴
                v8.AddHostObject("obj", HostItemFlags.GlobalMembers, obj);
                v8.Execute("var page = obj.Capture_Prop2");
                v8.Execute("var path = obj.Capture_Prop3");
                v8.Execute("var name = Boolean(obj.Capture_Prop4) ? obj.Capture_Prop4 : 'null'");
                v8.Execute("var extension = obj.Capture_Prop5");

                // 값들을 알맞은 형태로 변환하여 새로운 변수에 선언
                ElementHandle page = (ElementHandle)(v8.Evaluate("page"));
                string path = (string)(v8.Evaluate("path"));
                string name = (string)(v8.Evaluate("name"));
                string extension = (string)(v8.Evaluate("extension"));

                // 후에 작성할 파일 이름전용
                string full_name = "";
                
                // 파일 확장자가 png로 설정되어있는지 확인
                if ( extension == "png" )
                {
                    // 파일 이름이 없다면 Image.png 로, 있다면 파일이름 + .png 를 입력
                    full_name = String.IsNullOrWhiteSpace(name) || name == "null" ? "Image.png" : name + ".png";
                    // 스크린샷 옵션 중 png 는 퀄리티 사용이 불가능.
                    option = new ScreenshotOptions { Type = ScreenshotType.Png };
                }
                else
                {
                    // 파일 이름이 없다면 Image.jpeg 로, 있다면 파일이름 + .jpeg 를 입력
                    full_name = String.IsNullOrWhiteSpace(name) || name == "null" ? "Image.jpeg" : name + ".jpeg";
                    // 퀄리티는 기본값 0, 0으로 할 경우 알아보기 힘들 정도로 형태가 이상해짐.
                    option = new ScreenshotOptions { Quality = 100 , Type = ScreenshotType.Jpeg };
                }

                ScreenshotOptions oo = new ScreenshotOptions { Quality = 100 };

                // 다운로드 받을 경로를 합침
                path = System.IO.Path.Combine(path, full_name);

                // page는 해당 ElementHandle객체, 크롬창이 띄워져 있어야 캡쳐가 가능한걸로 확인
                // 디자이너에서도 테스트 가능 (ChromeFind 카드로 찾은 대상 RESULT.ScreenshotAsync(String.raw`C:\Users\hyper\Desktop\bin\asd.jpg`) )
                // 단, 디자이너 테스트 시 캡쳐파일의 확장자, 퀄리티는 사용할 수 없음.
                page.ScreenshotAsync(path, option);
            }
            // 오류 발생시 success 를 false로 설정하고, throw로 exception을 던짐.
            catch (Exception e)
            {
                success = false;
                throw e.InnerException;
            }

            // 결과값 리턴
            return success;
        }
    }
}
