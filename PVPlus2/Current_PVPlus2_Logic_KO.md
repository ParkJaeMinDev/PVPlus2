# 현재 PVPlus2 로직 정리

## 현재 상태

`PVPlus2`는 WPF 기반으로 다시 만드는 초기 단계 프로젝트다. 현재까지 구현된 것은 다음과 같다.

- 탭 기반 WPF 메인 화면
- `MainPV` 화면과 ViewModel 바인딩
- Excel, P, V, W 파일 선택 명령
- 간단한 텍스트 로그 출력
- `ExcelData` 데이터 컨테이너
- 과거 모델을 옮겨온 기본 도메인 모델들

아직 과거 PVPlus의 계산 파이프라인을 그대로 실행하는 단계는 아니다. 현재는 UI 연결과 데이터 로딩 기반을 만드는 중이다.

## 창 구조

`MainWindow.xaml`은 HandyControl `TabControl`로 4개 탭을 가진다.

- `MainPV`
- `Sample`
- `LTFHelper`
- `TabTest`

현재 실질적으로 작업 중인 화면은 `MainPV`다.

## MainPV 화면

`Views/MainPVView.xaml`에는 현재 다음 요소들이 있다.

- Excel 파일 경로 입력 행
- P/V/W 파일 경로 입력 행
- 공통 명령에 바인딩된 `열기` 버튼들
- `LoadExcelCommand`에 연결된 `출력` 버튼
- 상품코드, 회사명, 옵션, 라디오 선택을 위한 자리
- 하단 로그 출력용 읽기 전용 멀티라인 `TextBox`

레이아웃은 WPF `Grid`와 HandyControl 스타일을 사용한다.

## MainPVViewModel

`ViewModels/MainPVViewModel.cs`가 현재 `MainPV` 화면 상태를 관리한다.

### Observable 필드

- `엑셀파일경로`
- `P파일경로`
- `V파일경로`
- `W파일경로`
- `로그텍스트`

이 값들은 CommunityToolkit.Mvvm의 `[ObservableProperty]`를 통해 프로퍼티로 생성된다.

### private 데이터 컨테이너

- `_excelData`

이 필드는 `Models/ExcelData.cs`의 인스턴스다. 앞으로 로드된 참조 데이터를 메모리에 보관하는 용도로 사용될 예정이지만, 아직 본격적으로 채우지는 않는다.

### 명령

- `OpenFileCommand`
  - 파일 선택 대화상자를 연다.
  - `Excel`, `P`, `V`, `W` 값을 `CommandParameter`로 받아 어떤 경로 프로퍼티를 갱신할지 결정한다.
  - Excel은 엑셀 확장자 필터를 사용하고, 나머지는 전체 파일 필터를 사용한다.

- `LoadExcelCommand`
  - 시작 로그를 남긴다.
  - Excel 경로가 비어 있는지 검사한다.
  - 파일이 실제로 존재하는지 검사한다.
  - Sylvan.Data.Excel의 `ExcelDataReader`를 `ExcelSchema.NoHeaders` 옵션으로 생성한다.
  - 각 시트와 각 행을 순회한다.
  - 현재는 시트 이름과 셀 값을 로그에 출력한다.

즉 지금의 `LoadExcel()`은 최종 데이터 적재보다, Excel 읽기 동작을 먼저 확인하는 탐색 단계에 가깝다.

## 로그 출력

로그는 현재 문자열 누적 방식으로 처리한다.

- `AddLog(string message)`가 타임스탬프를 붙여 `로그텍스트`에 한 줄씩 이어 붙인다.
- UI는 `TextBox.Text`를 `로그텍스트`에 바인딩한다.

현재 타임스탬프 형식은 다음과 같다.

- `HH:mm:ss.fffff`

이 방식은 단순하고 안정적이며, 앞서 문제였던 `RichTextBox.Document` 바인딩 문제도 피할 수 있다.

## 모델 계층

현재 `Models` 폴더에는 다음 클래스들이 있다.

- `Product`
- `Rider`
- `Rate`
- `Layout`
- `VarChg`
- `Expense`
- `SInfo`
- `ChkExprs`
- `ExcelData`

### 현재 모델 설계 방향

수식 기반 컬럼들은 당장은 컴파일된 delegate가 아니라 `string`으로 저장하고 있다.

예:

- Rider 관련 수식
- Expense 조건 및 수식
- VarChg 수식
- ChkExprs 수식
- SInfo 수식

의도는 다음 3단계를 분리하는 것이다.

1. 원본 텍스트 로딩
2. 이후 수식 컴파일
3. 이후 계산 실행

이 방식은 과거 프로젝트처럼 로딩과 수식 컴파일이 강하게 섞여 있던 구조보다 단순하다.

## ExcelData 컨테이너

`ExcelData.cs`는 현재 로드된 데이터를 묶어 들고 있는 메모리 컨테이너다.

포함 내용은 다음과 같다.

- 원본 Excel 경로, Data 폴더 경로 같은 메타데이터
- 로드 시각
- Product, Rider용 사전
- Rate, Layout, VarChg, Expense, SInfo, ChkExprs용 그룹 사전

지금은 이 객체를 `MainPVViewModel`이 private 필드로 소유하고 있다. 즉 앱 전역 공유 데이터는 아니고 현재 화면 범위 데이터에 가깝다.

## 과거 PVPlus와의 현재 차이점

- WinForms 대신 WPF + MVVM
- 수동 이벤트 처리 대신 CommunityToolkit.Mvvm
- 전역 static 상태 대신 인스턴스 소유 데이터 지향
- `List` 중심 조회 대신 `Dictionary` 기반 조회를 계획
- 현대 Excel 읽기를 위해 Sylvan.Data.Excel 사용
- 수식 필드는 아직 컴파일하지 않고 문자열 상태로 유지

## 현재 한계

- `LoadExcel()`은 아직 `ExcelData`를 채우지 않고 로그만 출력한다.
- 과거 계산 클래스들은 아직 포팅되지 않았다.
- 회사 규칙, layout, 위험률, 사업비, 검증식이 실행 흐름에 아직 연결되지 않았다.
- 전용 loader 클래스도 아직 없다.
- 데이터 파싱과 인덱싱 방식은 아직 확정 중이다.

## 다음 단계

가장 자연스러운 다음 단계는 아래 순서다.

1. Excel을 목적에 맞게 읽는다.
2. 필요한 시트 또는 내보낸 데이터를 식별한다.
3. `ExcelData`를 실제 데이터로 채운다.
4. 그 위에 `Dictionary` 기반 조회 로직을 추가한다.
