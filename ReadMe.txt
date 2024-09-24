Rule
 * 파일명 규칙 : BrityRPA.AddIn.[AddInName].dll
 * AddIn 메인 클래스
    - 클래스 이름 : BrityRPA.AddIn.[AddInName].AddIn
    - BrityRPA.AddIn.BaseActivityAddIn 상속
 * Activity 클래스
    - IActivity 인터페이스 구현
    - AddIn.CreateActivites() 에서 Activity 등록

1) 새 프로젝트 생성 
  - 프로젝트 타입: 클래스 라이브러리(.NET Framework)
  - 프로젝트 명: BrityRPA.AddIn.[AddInName]
  - 프레임워크 버전: .NET Framework 4.5
2) 프로젝트 속성 설정
  - 빌드 > 출력 경로: 디자이너_설치_폴더/AddIns/
  - 디버그 > 시작 외부 프로그램: 디자이너 exe 경로
  - 디버그 > 시작 옵션 > 작업 디렉터리: 디자이너 설치 폴더
  - 리소스 > 리소스 추가 (AddIn 기본 아이콘, 마우스 오버시 표시할 아이콘, 각 Activity 별 아이콘)
  
  * 디자이너 실행 모드 변경
    Designer.config 파일에서 PlayInBot 속성의 value 값을 true 에서 false 로 변경
     => <UserSetting category="Designer" group="Engine" name="PlayInBot" value="true" isReadOnly="false" valueType="bool" />
    변경시 BrityRPA_BOT.exe 대신 BrityRPA_Designer.exe 가 엔진을 직접 구동하는 방식으로 변경되어 디자이너에서 Run 수행시 AddIn 프로젝트 디버깅 가능
    
3) 문자열 리소스 파일 추가
  - 프로젝트/Resources/String/ 아래 언어별 파일 추가 (String-en-US.xaml, String-ko-KR.xaml)
  - 프로퍼티 표시 문자열 작성 규칙(샘플 참조)
  - Property Group & Name 문자열 리소스(String-ko-KR.xaml) 포멧
    a) Resource Key Format : '_' 를 기준으로 그룹과 이름을 구분한다.
     - Format 1 : PROP_[PropertyKey] => 프로그램 전체 공통 속성
     - Format 2 : PROP_[LibraryType]_[PropertyKey] => 특정 타입의 모든 라이브러리의 공통 속성
     - Format 3 : PROP_[LibraryType]_[PropertyKey]_[LibraryName] => 특정 라이브러리 속성
     * 우선순위는 3 -> 2 -> 1 순으로 찾는다.
    b) Resource Contents format : '\' 를 기준으로 그룹과 이름을 표시한다.
     - Format : [Display Property Group]\[Display Property Name]
     * Property 뷰는 Resource Contents format의 'Display Property Group' 으로 그룹핑한다.
  - Property Description 문자열 리소스(String-ko-KR.xaml) 포멧
     * Property Group & Name 포멧의 Prefix 만 다름(PROP -> DESC)
     - Format 1 : DESC_[PropertyKey] => 프로그램 전체 공통 속성
     - Format 2 : DESC_[LibraryType]_[PropertyKey] => 특정 타입의 모든 라이브러리의 공통 속성
     - Format 3 : DESC_[LibraryType]_[PropertyKey]_[LibraryName] => 특정 라이브러리 속성


4) 참조 추가
  - 디자이너설치폴더의 IPA.Common.dll, IPA.AddIn.dll 추가
  - 추가한 참조의 속성 중 '로컬 복사' 항복 False 로 변경
5) AddIn.cs 파일 생성
  - RPAGO.AddIn.ActivityAddInBase 상속 받아 구현
6) 프로젝트/Activities 폴더 생성
7) SampleActivity.cs 파일 생성
  - RPAGO.AddIn.IActivityItem 인터페이스 상속 받아 구현
    - OnCreateProperties() 구현시 프로퍼티 뷰에 표시할 속성 목록 생성
    - OnRun() 에서 실행 시, 동작 구현
  - AddIn.CreateActivities() 에서 반환할 Activity 리스트에 SampleActivity 생성하여 추가

