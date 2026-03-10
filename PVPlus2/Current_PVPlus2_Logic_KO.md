# 현재 PVPlus2 로직 정리

## 현재 상태

`PVPlus2`는 과거 PVPlus를 WPF로 다시 만드는 초기 단계 프로젝트다.

현재까지 구현된 것은 다음과 같다.

- HandyControl 탭 기반 메인 창
- `MainPV` 화면과 `MainPVViewModel` 바인딩
- Excel, P, V, W 파일 선택 명령
- `상품코드` 입력 바인딩
- `구분자체크` 체크박스 바인딩
- 문자열 누적 방식 로그 출력
- `ExcelData` 메모리 컨테이너
- Excel 로드를 담당하는 `ExcelDataLoader` 서비스
- Parlot 기반의 최소 수식 파서를 담은 `ExpressionCompiler` 서비스

아직 계산 파이프라인은 연결되지 않았고, 현재는 Excel 로더 구조와 수식 컴파일러 기반을 만드는 단계다.

## 창 구조

`MainWindow.xaml`은 HandyControl `TabControl`로 4개 탭을 가진다.

- `MainPV`
- `Sample`
- `LTFHelper`
- `TabTest`

현재 실질적으로 작업 중인 화면은 `MainPV`다.

## MainPV 화면

`Views/MainPVView.xaml`에는 현재 다음 요소들이 있다.

- Excel 파일 경로 입력과 열기 버튼
- P/V/W 파일 경로 입력과 열기 버튼
- `LoadExcelCommand`에 연결된 `출력` 버튼
- `상품코드` TwoWay 바인딩 입력칸
- `구분자체크` TwoWay 바인딩 체크박스
- 회사/옵션/라디오/기타 버튼용 자리 UI
- 하단 읽기 전용 로그 `TextBox`

`Views/MainPVView.xaml.cs`는 현재도 생성자에서 `DataContext = new MainPVViewModel();`를 설정한다.

## MainPVViewModel

`ViewModels/MainPVViewModel.cs`는 예전보다 역할이 줄어든 상태다.

### Observable 필드

- `엑셀파일경로`
- `P파일경로`
- `V파일경로`
- `W파일경로`
- `로그텍스트`
- `상품코드`
- `구분자체크`

### 현재 역할

현재 `MainPVViewModel`은 다음을 담당한다.

- UI 상태 보관
- 파일 선택 명령
- `LoadExcelCommand`
- `AddLog(string message)`를 통한 로그 누적

### 현재 Excel 로드 흐름

`LoadExcel()`은 더 이상 직접 시트를 읽지 않는다.

현재는 다음 순서로 동작한다.

1. `ExcelDataLoader`를 생성한다.
2. `AddLog` 메서드를 서비스에 넘긴다.
3. `loader.LoadExcel(엑셀파일경로, 상품코드, 구분자체크)`를 호출한다.
4. 서비스가 null이 아닌 `ExcelData`를 돌려주면 `_excelData`를 교체한다.

## ExcelDataLoader 서비스

`Services/ExcelDataLoader.cs`가 현재 Excel workbook 로드와 시트 분기를 담당한다.

### 서비스 입력과 내부 상태

서비스는 현재 다음 값을 입력으로 받는다.

- Excel 파일 경로
- 상품코드
- 구분자 체크 여부
- 선택적 로그 콜백 (`Action<string>`)

로드 1회 동안 상품코드와 구분자 체크 상태를 private field에 저장해서 사용한다.

### Workbook 열기

`LoadExcel(...)`은 현재 다음을 수행한다.

- 상품코드 공란 검사
- Excel 경로 공란 검사
- 파일 존재 여부 검사
- 새로운 `ExcelData` 생성
- `Sylvan.Data.Excel`로 workbook 열기
- worksheet 순회
- 시트 이름별 분기 호출

### 현재 시트 분기 대상

- `Layout`
- `Product`
- `Rider`
- `Rate`
- `Expense`
- `VarChg`
- `SInfo`
- `ChkExprs`

알 수 없는 시트 이름은 조용히 건너뛴다.

### 현재 구현된 시트 로더

#### `LoadLayoutSheet`

`Layout` 로더는 현재 실제 적재가 들어간 부분 구현 상태다.

현재 동작은 다음과 같다.

- 처음 2행을 헤더로 보고 건너뜀
- 한 행을 P/V/S 세 블록으로 나눠 읽음
  - P 시작 열 0
  - V 시작 열 7
  - S 시작 열 14
- 상품코드가 다음 중 하나인 행만 포함
  - `RiderCode`
  - `Check`
  - `Base`
  - 현재 입력한 상품코드
- `FactorName`이 빈칸이면 제외
- 구분자 모드면 `Index`가 빈칸인 행 제외
- 고정폭 모드면 `Start`가 빈칸인 행 제외
- `Start`, `Length`, `Index`는 `ToIntOrDefault(..., 0)`으로 변환
- 결과를 `_excelData.PLayout`, `_excelData.VLayout`, `_excelData.SLayout`에 적재
- 저장 구조는 `Dictionary<string, List<Layout>>`
- 즉 상품코드별로 여러 `Layout`을 그룹화해서 저장

이 필터 조건은 과거 PVPlus의 `ReadLayouts()` 핵심 규칙을 반영한 것이다.

#### `LoadProductSheet`

`Product` 로더도 현재 부분 구현 상태다.

현재 동작은 다음과 같다.

- `Product` 시트를 한 행씩 읽음
- 첫 번째 컬럼의 상품코드가 현재 입력 상품코드와 일치하는 첫 행을 찾음
- 다음 값을 읽음
  - `상품코드`
  - `판매시기`
  - `상품명`
  - `예정이율`
  - `평균공시이율`
  - `판매채널`
- `Product` 객체를 만들어 `_excelData.Product`에 저장
- 로드된 값을 로그로 남김
- 파싱 실패 시 오류 로그 후 종료
- 끝까지 못 찾으면 not-found 로그를 남김

현재 구현은 Product 시트의 숫자 셀이 실제 숫자 셀로 들어 있다는 전제를 가진다.

### 아직 비어 있는 시트 로더

다음 메서드들은 아직 루프 골격만 있고 실제 적재 로직은 없다.

- `LoadRiderSheet`
- `LoadRateSheet`
- `LoadExpenseSheet`
- `LoadVarChgSheet`
- `LoadSInfoSheet`
- `LoadChkExprsSheet`

## 모델 설계 방향

현재 모델 방향은 “먼저 raw 데이터나 단순 스칼라 값을 적재하고, 이후 compile/runtime 계층을 붙이는 방식”이다.

예를 들면:

- `Product`는 단순 수치 필드는 숫자 타입으로 보관
- `Rider`는 수식 관련 필드를 현재 모두 `string`으로 보관
- `RateKeyByRateVariable`은 이미 `Dictionary<string, string>` 구조 사용
- `ExcelData`는 도메인별 데이터를 묶는 public 컨테이너

이 방향은 과거 PVPlus처럼 로딩과 수식 컴파일이 한 덩어리였던 구조와 의도적으로 다르다.

## ExpressionCompiler

`Services/ExpressionCompiler.cs`는 Parlot 기반의 첫 번째 수식 파서 초안이다.

### 현재 구현된 기능

현재 지원하는 것은 아주 최소한이다.

- 숫자 리터럴
- 괄호
- 이항 `+`
- 이항 `-`
- 이항 `*`
- 이항 `/`

### 현재 설계 방향

현재 컴파일러는 단순화를 위해 다음 방향을 따른다.

- 모든 숫자형 계산은 `double` 기준
- 평가 결과 타입도 `double`
- static compiled parser를 재사용
- `Eof()`를 적용해서 문자열 전체를 모두 소비해야 성공
- 아직 custom AST 생성 단계는 아니고 즉시 파싱/평가 수준

즉 새 엔진 방향에서는 `1 / 1000`을 `0.001`로 계산하는 쪽을 목표로 한다.

### 아직 미구현된 연산자

Flee 기준으로 아직 없는 연산자는 다음과 같다.

- `%`
- `^`
- `=`
- `<>`
- `<`
- `>`
- `<=`
- `>=`
- `And`
- `Or`
- `Xor`
- `Not`
- `<<`
- `>>`

완전히 구현되지 않은 것:
- 일반 unary minus 예: `-(1+2)`
- unary plus

### 아직 미구현된 함수

현재 함수 호출 자체가 없다.

즉 아직 지원하지 않는 예시는 다음과 같다.

- `If(...)`
- `Abs(...)`
- `Min(...)`
- `Max(...)`
- `Round(...)`
- `Floor(...)`
- `Ceiling(...)`
- `Pow(...)`
- `cast(...)`
- `in`
- 프로젝트 전용 helper 함수들

### 아직 미구현된 변수 참조와 런타임 바인딩

현재 컴파일러는 변수 참조를 전혀 지원하지 않는다.

즉 아직 사용할 수 없는 예시는 다음과 같다.

- factor 변수 `F1 ~ F10`
- 위험률 변수 `q1 ~ q30`
- MP 변수 `n`, `m`, `Age`, `Freq`, `Jong`, `ElapseYear`
- S 변수 `S1 ~ S10`
- 체크 변수 `NP0`, `GP0`, `V0`, `W0`
- 임시 변수 `TempStr1`, `TempCK0`

또한 아직 지원하지 않는 것:

- 배열 인덱싱 예: `VWhole[0]`
- 멤버 접근
- 문자열 수식
- bool 수식
- 혼합 타입 수식
- 변수 컨텍스트 기반 delegate 생성

## 현재 한계

현재 한계는 다음과 같다.

- `Layout`, `Product`만 부분 구현됨
- `Rider`, `Rate`, `Expense`, `VarChg`, `SInfo`, `ChkExprs` 로더는 미구현
- `ExpressionCompiler`는 아직 최소 사칙연산 프로토타입 수준
- 변수 참조가 가능한 평가 엔진이 아직 없음
- Flee 호환 수식 런타임이 아직 없음
- 계산 파이프라인과 아직 연결되지 않음

## 다음 방향

가장 자연스러운 다음 단계는 아래 순서다.

1. 현재 raw-string 모델 기준으로 `LoadRiderSheet` 구현
2. 나머지 시트 로더 구현
3. `ExpressionCompiler`에 변수와 함수 지원 추가
4. 로드된 rule 데이터 위에 런타임 평가 계층 구축
5. 이후 계산 파이프라인 연결
