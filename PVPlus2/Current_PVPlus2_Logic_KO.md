# 현재 PVPlus2 로직 정리

## 현재 상태

`PVPlus2`는 여전히 과거 PVPlus를 WPF로 다시 만드는 초기 단계 프로젝트다.

현재까지 구현된 것은 다음과 같다.

- HandyControl 탭 기반 메인 창
- `MainPV` 화면과 `MainPVViewModel` 바인딩
- `TestView`와 `TestViewModel`을 사용하는 `TabTest` 테스트 화면
- Excel, P, V, W 파일 선택 명령
- `상품코드` 입력 바인딩
- `구분자체크` 체크박스 바인딩
- 문자열 로그 출력 영역
- `ExcelData` 메모리 컨테이너
- Excel 로드를 담당하는 `ExcelDataLoader` 서비스
- `System.Linq.Expressions` 기반 delegate를 생성하는 `ExpressionCompiler` 서비스

아직 계산 파이프라인은 연결되지 않았고, 현재는 Excel 로더와 수식 런타임의 기반을 만드는 단계다.

## 창 구조

`MainWindow.xaml`은 HandyControl `TabControl`로 4개 탭을 가진다.

- `MainPV`
- `Sample`
- `LTFHelper`
- `TabTest`

현재 업무 화면은 `MainPV`이고, `TabTest`는 파서, 런타임, 정확도 실험용 화면이다.

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

이 필터 조건은 과거 PVPlus의 layout 로딩 핵심 규칙을 반영한 것이다.

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

## 수식 런타임 구조

현재 수식 런타임은 더 이상 예전의 `x`, `y` 전용 프로토타입이 아니다.

지금은 다음 구조를 기준으로 동작한다.

- 고정된 `ExpressionContext` 모델
- static `ExpressionCompiler`
- `System.Linq.Expressions` 기반 delegate 생성
- 대소문자 무시 property/function lookup

## ExpressionContext

`Models/ExpressionContext.cs`가 현재 수식 입력 모델이다.

현재 형태:

- `a`부터 `z`까지 public `double` property

현재 의미:

- 모든 수식은 `ExpressionContext` 기준으로 compile된다.
- 식별자는 이 타입의 public instance property로 해석된다.
- property lookup은 대소문자를 구분하지 않는다.

예:

- `x + y`
- `X + Y`
- `a * 3`

위 수식들은 모두 `ExpressionContext`의 property를 읽는다.

## ExpressionFunctions

`Services/ExpressionFunctions.cs`는 현재 사용자 정의 함수 컨테이너다.

현재 상태:

- class 자체가 `static`
- public static 메서드를 reflection으로 수집
- 함수 이름 lookup은 대소문자 무시
- 현재 테스트용 메서드는 다음 두 개뿐이다.
  - `test(long a)`
  - `test(double a)`

함수 호출 파싱 구조는 이미 들어가 있지만, 현재 벤치마크 수식에는 아직 custom function을 넣지 않은 상태다.

## ExpressionCompiler

`Services/ExpressionCompiler.cs`가 현재 파서/컴파일러의 중심이다.

### Compile 진입점

현재 공개 메서드는 다음과 같다.

- `CompileDouble(string text)` -> `Func<ExpressionContext, double>`
- `CompileLong(string text)` -> `Func<ExpressionContext, long>`
- `CompileBool(string text)` -> `Func<ExpressionContext, bool>`

현재 동작:

- 입력 문자열을 trim한다.
- parser가 `System.Linq.Expressions.Expression`을 만든다.
- 최종 body를 `ExpressionContext context` 하나를 받는 lambda로 감싼다.
- `CompileDouble`, `CompileLong`은 마지막 결과를 요청된 타입으로 명시적으로 변환한다.
- `CompileBool`은 최종 body 타입이 이미 `bool`이어야만 한다.

### 지원 리터럴

현재 지원:

- 정수 리터럴 -> `long`
  - 예: `1`, `2`, `100`
- 소수 리터럴 -> `double`
  - 예: `1.0`, `10.5`, `.5`
- bool 리터럴
  - `True`
  - `False`
  - 대소문자 무시

현재 미지원:

- scientific notation
  - `1e10`
  - `1e-5`
- 천 단위 구분 기호
  - `1,000`
- string literal
- `2.75%` 같은 퍼센트 literal

현재 `%`는 오직 modulo 연산자로만 사용된다.

### 지원 산술 연산자

현재 지원:

- unary `+`
- unary `-`
- binary `+`
- binary `-`
- binary `*`
- binary `/`
- binary `%`
- binary `^`

현재 핵심 규칙:

- `^`는 `Expression.Power(...)`로 구현
- `^`는 오른쪽 결합
  - `2 ^ 3 ^ 2`는 `2 ^ (3 ^ 2)` 의미
- `%`는 숫자 나머지 연산
- `/`는 항상 `double` 나눗셈으로 승격

현재 숫자 승격 규칙:

- `long op long`은 `+`, `-`, `*`, `%`에서 `long` 유지
- `long`과 `double`이 섞이면 `double`로 승격
- `/`는 양쪽 모두 `double`로 변환
- `^`도 양쪽 모두 `double`로 변환

### 지원 비교 연산자

현재 지원:

- `=`
- `==`
- `!=`
- `<>`
- `>`
- `>=`
- `<`
- `<=`

현재 의미:

- `=`과 `==`는 모두 같음
- `!=`와 `<>`는 모두 다름
- 관계 비교 (`>`, `>=`, `<`, `<=`)는 숫자형끼리만 지원
- 같음/다름 비교는 다음 조합을 지원
  - 숫자 vs 숫자
  - bool vs bool

예:

- `1 = 1`
- `1 == 1`
- `1 <> 2`
- `1 != 2`
- `x >= y`

### 지원 논리 연산자

현재 지원:

- `NOT`
- `AND`
- `OR`

모두 대소문자를 구분하지 않는다.

예:

- `NOT (1 == 2)`
- `TRUE AND NOT FALSE`
- `x > y OR y > x`

현재 의미:

- `NOT`은 bool operand만 허용
- `AND`, `OR`는 bool operand만 허용
- 내부적으로 `Expression.Not`, `Expression.AndAlso`, `Expression.OrElse`를 사용

### 현재 우선순위

현재 parser 우선순위는 다음과 같다.

1. primary
   - literal
   - identifier
   - function call
   - 괄호식
2. unary
   - `+`
   - `-`
3. power
   - `^` (오른쪽 결합)
4. multiplicative
   - `*`
   - `/`
   - `%`
5. additive
   - `+`
   - `-`
6. relational
   - `>`
   - `>=`
   - `<`
   - `<=`
7. equality
   - `=`
   - `==`
   - `!=`
   - `<>`
8. logical not
   - `NOT`
9. logical and
   - `AND`
10. logical or
   - `OR`

### 대소문자 처리

현재 compiler는 사용자 표현식에서 가능한 부분은 대소문자를 구분하지 않도록 설계돼 있다.

현재 대소문자 무시 대상:

- `ExpressionContext` property lookup
- `ExpressionFunctions` function lookup
- `True`, `False`
- `AND`, `OR`, `NOT`

예:

- `X + y`
- `true or FALSE`
- 함수가 실제 테스트 수식에 들어갈 경우 `TeSt(1)`도 같은 함수로 lookup된다.

기호 연산자 자체는 당연히 대소문자와 무관하다.

### 함수 호출 지원

현재 문법:

- `name(arg1, arg2, ...)`

현재 binding 규칙:

- 함수 이름은 `ExpressionFunctions`에서 찾음
- public static method만 허용
- overload resolution 순서
  - exact type match 우선
  - 그 다음 `long -> double`
  - 그 다음 `double -> long`
- 같은 점수의 overload가 둘 이상이면 ambiguous로 처리

### 현재 런타임 특이점

현재 구현에는 몇 가지 중요한 edge behavior가 있다.

- `1 / 0`은 `CompileDouble`에서 예외가 아니라 `Infinity`
  - `/`가 `double` 나눗셈으로 승격되기 때문
- `0 / 0`은 `NaN`
- `1 % 0`은 `DivideByZeroException`이 날 수 있음
  - integer remainder 경로가 남아 있기 때문
- `--1`은 현재 정상 파싱된다.
- unary plus는 현재 pass-through 동작
  - 그래서 `+True`도 현재는 컴파일된다.
- `1 < 2 < 3` 같은 chained comparison은 지원하지 않는다.
  - 두 번째 비교에서 타입 불일치 예외가 난다.

### 현재 미구현

아직 다음은 구현되지 않았다.

- string expression
- string comparison
- scientific notation 숫자
- 천 단위 구분자 숫자
- percent literal
- ternary syntax
- 전용 `If(...)` 지원
- test 함수 외의 실제 함수군
- `ExpressionContext`를 넘는 업무 도메인 변수 바인딩
- 일반 배열 인덱싱
- legacy Flee와의 완전한 기능 호환

## TestView와 테스트 하네스

`Views/TestView.xaml`은 현재 `TabTest` 탭에서 사용하는 파서/런타임 테스트 화면이다.

### 현재 UI

현재 화면에는 다음 요소들이 있다.

- `RunTestParlotCommand`에 연결된 `Parlot` 버튼
- `TotalTestCommand`에 연결된 `TotalTest` 버튼
- `ArrayLength`에 바인딩된 `Array Length` 입력칸
- 읽기 전용 멀티라인 `InputText`
- 읽기 전용 멀티라인 `OutputText`

`Views/TestView.xaml.cs`는 생성자에서 `DataContext = new TestViewModel();`를 설정한다.

### 현재 TotalTest 흐름

`ViewModels/TestViewModel.cs`는 더 이상 compile 시간 벤치마크를 하지 않는다.

현재 `TotalTest()`는 다음을 수행한다.

- `OutputText` 초기화
- `ArrayLength > 0` 검사
- 수식을 4개 그룹으로 구성
  - 정상 Numeric
  - 오류 예상 Numeric
  - 정상 Boolean
  - 오류 예상 Boolean
- 정상 Numeric 수식을 `CompileDouble`로 1회씩 compile
- 정상 Boolean 수식을 `CompileBool`로 1회씩 compile
- 랜덤 `xValues`, `yValues` 생성
- 컴파일된 Numeric 수식을 반복 평가
- 같은 의미의 native C# Numeric lambda를 반복 평가
- 컴파일된 Boolean 수식을 반복 평가
- 같은 의미의 native C# Boolean lambda를 반복 평가
- 오류 예상 수식은
  - compile
  - 1회 evaluate
  - 예외 발생 시 정상 통과
  - 예외가 없으면 `Unexpected success`
로 판정

### 현재 출력 섹션

`TotalTest()`는 현재 다음을 출력한다.

- 정상 Numeric 평가 시간 표
- 정상 Numeric checksum 비교 표
- 정상 Boolean 평가 시간 표
- 정상 Boolean true-count 비교 표
- 오류 예상 Numeric 검증 표
- 오류 예상 Boolean 검증 표

### 현재 checksum 정책

Numeric 검증은 다음을 비교한다.

- compiler checksum
- native checksum
- absolute difference
- match 여부

특수값은 별도로 처리한다.

- `NaN` vs `NaN` -> match
- `+Infinity` vs `+Infinity` -> match
- `-Infinity` vs `-Infinity` -> match

Boolean 검증은 다음을 비교한다.

- 반복 실행 전체에서 `true`가 나온 횟수

### 현재 테스트 목적

현재 테스트 화면은 단순한 microbenchmark 화면만은 아니다.

이제는 다음 역할도 같이 한다.

- parser regression check
- native C#와의 의미 일치 검증
- 예상 오류 수식 검증

## 현재 한계

현재 한계는 다음과 같다.

- `Layout`, `Product`만 부분 구현됨
- `Rider`, `Rate`, `Expense`, `VarChg`, `SInfo`, `ChkExprs` 로더는 미구현
- 수식 런타임은 아직 실제 업무 rule 실행과 분리된 상태
- `ExpressionContext`는 아직 테스트 중심의 고정 property bag
- 함수 호출 구조는 있지만 실제 함수군은 test 메서드 수준
- string/객체 기반 수식 모델이 아직 없음
- legacy 계산 클래스와 아직 연결되지 않음

## 다음 방향

가장 자연스러운 다음 단계는 아래와 같다.

1. 나머지 Excel 시트 로더 구현
2. `ExpressionContext`를 넘어서는 실제 업무 변수 바인딩 설계
3. `ExpressionFunctions`에 실제 함수군 추가
4. edge case 중심 테스트를 업무 규칙 중심 테스트로 확장
5. 로드된 rule 데이터와 런타임 평가 계층 연결
6. 이후 계산 파이프라인 연결
