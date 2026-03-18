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
- Parlot AST 파싱 단계와 AST -> `System.Linq.Expressions.Expression` 바인딩 단계
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

현재 등록된 함수 목록:

- `Min(params double[])`, `Max(params double[])` — 가변 인수, `double` 반환
- `Abs(long)`, `Abs(double)`
- `Floor(long)`, `Floor(double)`
- `Ceiling(long)`, `Ceiling(double)`
- `Round(long)`, `Round(double)` — `MidpointRounding.AwayFromZero` 사용
- `Round(long, long)`, `Round(double, long)` — digits 인수 버전, 동일하게 `AwayFromZero`
- `Pow(long, long)`, `Pow(double, double)`, `Pow(long, double)`, `Pow(double, long)`
- `Sqrt(long)`, `Sqrt(double)`
- `Truncate(long)`, `Truncate(double)`
- `Sign(long)`, `Sign(double)` — `long` 반환
- `test(long)`, `test(double)` — 테스트 전용

## ExpressionCompiler

`Services/ExpressionCompiler.cs`가 현재 파서/컴파일러의 중심이다.

### Compile 진입점

현재 공개 메서드는 다음과 같다.

- `CompileDouble(string text)` -> `Func<ExpressionContext, double>`
- `CompileLong(string text)` -> `Func<ExpressionContext, long>`
- `CompileBool(string text)` -> `Func<ExpressionContext, bool>`
- `CompileString(string text)` -> `Func<ExpressionContext, string>`

현재 동작:

- 입력 문자열을 trim한다.
- parser가 먼저 `AstNode` 트리를 만든다.
- `BindSyntax(...)`가 AST를 `System.Linq.Expressions.Expression`으로 변환한다.
- 최종 body를 `ExpressionContext context` 하나를 받는 lambda로 감싼다.
- `CompileDouble`, `CompileLong`은 마지막 결과를 요청된 타입으로 명시적으로 변환한다.
- `CompileBool`은 최종 body 타입이 이미 `bool`이어야만 한다.
- `CompileString`은 최종 body 타입이 이미 `string`이어야만 한다.

### 지원 리터럴

현재 지원:

- 정수 리터럴 -> `long`
  - 예: `1`, `2`, `100`
- 소수 리터럴 -> `double`
  - 예: `1.0`, `10.5`, `.5`
- string 리터럴 -> AST에서는 `Parlot.TextSpan`으로 보관하고 바인딩 시 `string`으로 materialize
  - 예: `"hello"`, `"A"`, `"literal with space"`
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
- `+`는 좌우 중 하나가 `string`이면 문자열 concat으로 해석
- 현재 문자열 concat은 pairwise `string.Concat(object, object)`로 처리
- string은 binary `+`에서만 허용

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
  - string vs string
- string과 비-string이 섞인 같음/다름 비교는 예외 처리

예:

- `1 = 1`
- `1 == 1`
- `1 <> 2`
- `1 != 2`
- `x >= y`
- `"a" = "a"`
- `"a" <> "b"`

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

### 특수 내장 함수

현재 compiler는 reflection 함수 경로와 별도로 특수 함수 전용 바인더 계층을 가진다.

#### `if`

문법:

- `if(condition, true_value, false_value)`

규칙:

- 인수는 정확히 3개
- `condition`은 반드시 `bool`
- `true_value`와 `false_value`는 같은 타입이거나, 둘 다 숫자형(`double` 승격)
- `Expression.Condition(...)`으로 컴파일 — 런타임에는 선택된 브랜치만 평가
- `if(True, 1, "x")` 같은 타입 불일치 브랜치는 컴파일 시 오류

예:

- `if(x > 5, x * 2, 0.0)`
- `if(True, "A", "B")`
- `if(x > y, x + y, x - y)`

#### `ifs`

문법:

- `ifs(condition1, value1, condition2, value2, ..., conditionN, valueN, default_value)`

규칙:

- 인수 개수는 홀수, 최소 3개
- 각 `conditionN`은 반드시 `bool`
- 모든 `valueN`과 `default_value`는 같은 타입이거나, 모두 숫자형(`double` 승격)
- 오른쪽에서 왼쪽으로 중첩 `Expression.Condition(...)`으로 컴파일 — 런타임에는 일치하는 브랜치와 해당 값만 평가
- 첫 번째 일치 이후의 브랜치는 절대 평가되지 않음

예:

- `ifs(x > 10, "high", x > 5, "mid", "low")`
- `ifs(x > y + 100, x + y, x > y, x - y, y - x)`

#### `cast`

문법:

- `cast(value, type)`

규칙:

- 인수는 정확히 2개
- 두 번째 인수는 타입명 identifier여야 함
- 지원 타입 alias:
  - `int`, `int32` → `long`
  - `long`, `int64` → `long`
  - `double`, `float64` → `double`
  - `bool`, `boolean` → `bool`
  - `string` → `string`
- `float`, `single`은 명시적으로 미지원 (컴파일 오류)
- 숫자↔숫자 변환은 `Expression.Convert` 사용
- 동일 타입 cast는 항등
- 비숫자 타입 간 변환은 컴파일 시 오류
- `double` → `long` 경계값(NaN, Infinity) 동작은 CLR에 위임

예:

- `cast(1.9, int)` → `1L`
- `cast(x, double)` → `x`가 이미 `double`이면 항등

#### 함수 바인더 아키텍처

`CreateFunctionCallExpression`이 현재 함수 바인딩의 단일 진입점이다.

동작 순서:

1. `TryBindSpecialFunction(name, rawArguments, out expression)`을 먼저 호출
2. `true`를 반환하면 해당 특수 표현식을 바로 반환 (인수는 아직 바인딩되지 않은 상태)
3. 아니면 모든 인수를 `BindSyntax`로 바인딩한 뒤 `CreateReflectionFunctionCallExpression`으로 전달

이 구조 덕분에 특수 함수는 `AstNode` 미바인딩 상태로 인수를 받을 수 있어 short-circuit 의미가 보장된다.

### 일반 함수 호출 지원

현재 문법:

- `name(arg1, arg2, ...)`
- 최소 인수 1개 필요 — `fn()` 형태는 파서 단계에서 `FormatException`으로 거부됨

현재 binding 규칙:

- 함수 이름은 `ExpressionFunctions`에서 찾음
- public static method만 허용
- overload resolution 점수:
  - 완전 일치: 0
  - `long` → `double`: 1
  - `double` → `long`: 1
  - `IsAssignableFrom` / `object`: 2
  - `params` 후보 penalty: +1
- 모든 후보를 끝까지 순회한 뒤 최종 ambiguous 판정 — 루프 중간에 즉시 예외를 던지지 않음
- 최저 score가 순회 종료 후에도 복수 후보에 해당하면 ambiguous 오류

### `params T[]` 지원

마지막 파라미터가 `params T[]`인 메서드를 지원한다.

규칙:

- 마지막 파라미터의 `ParamArrayAttribute` 감지로 판별
- `arguments.Count >= 고정파라미터수` 조건 필요
- 고정 파라미터는 개별적으로 기존 방식 변환
- trailing 인수는 각각 원소 타입으로 변환 후 `Expression.NewArrayInit(...)`으로 묶음
- `params` 후보는 score에 +1 penalty — 고정 arity 오버로드가 항상 우선

`ExpressionFunctions` 작성 시 원소 타입 선택 기준:

| 원소 타입 | 용도 |
|---|---|
| `double[]` | 숫자 계산 함수 기본값 |
| `long[]` | 정수 반환이 명시적으로 필요한 경우 |
| `object[]` | 이종 인수 함수 전용 — 숫자 함수에 사용 금지 |

### 현재 런타임 특이점

현재 구현에는 몇 가지 중요한 edge behavior가 있다.

- `1 / 0`은 `CompileDouble`에서 예외가 아니라 `Infinity`
  - `/`가 `double` 나눗셈으로 승격되기 때문
- `0 / 0`은 `NaN`
- `1 % 0`은 `DivideByZeroException`이 날 수 있음
  - integer remainder 경로가 남아 있기 때문
- `--1`은 현재 정상 파싱된다.
- unary plus는 string이 아닌 타입에서는 pass-through 동작
  - 그래서 `+True`도 현재는 컴파일된다.
  - `+"a"`는 허용되지 않는다.
- `1 < 2 < 3` 같은 chained comparison은 지원하지 않는다.
  - 두 번째 비교에서 타입 불일치 예외가 난다.
- `"a" > "b"` 같은 string 관계 비교는 지원하지 않는다.
- `"a" == 1` 같은 string/비-string 혼합 equality는 지원하지 않는다.

### 현재 미구현

아직 다음은 구현되지 않았다.

- scientific notation 숫자
- 천 단위 구분자 숫자
- percent literal
- string 관계 비교
- literal/concat/equality 외의 일반 string 함수군
- `ExpressionContext` 인식 함수 주입 (예정: 첫 파라미터 `_contextParameter` 자동 주입)
- `ExpressionContext`를 넘는 업무 도메인 변수 바인딩
- 일반 배열 인덱싱
- legacy Flee와의 완전한 기능 호환
- 0인수 함수 호출 문법 (`fn()` 형태는 현재 정책상 파서 단계에서 거부)

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

`ViewModels/TestViewModel.cs`는 이제 typed regression + microbenchmark 하네스 역할을 한다.

현재 `TotalTest()`는 다음을 수행한다.

- `OutputText` 초기화
- `ArrayLength > 0` 검사
- 수식을 8개 그룹으로 구성
  - 정상 Double
  - 오류 예상 Double
  - 정상 Long
  - 오류 예상 Long
  - 정상 Boolean
  - 오류 예상 Boolean
  - 정상 String
  - 오류 예상 String
- 모든 그룹을 정리한 `InputText`를 다시 구성
- 정상 수식에 대해 compile 시간 벤치마크 수행
  - `CompileDouble`
  - `CompileLong`
  - `CompileBool`
  - `CompileString`
- 마지막 compile 결과를 타입별 cache에 저장
- 고정 seed 기반 랜덤 `xValues`, `yValues` 생성
- 네 타입 모두에 대해 compiler delegate 평가 시간 측정
- 네 타입 모두에 대해 native C# delegate 평가 시간 측정
- 오류 예상 수식은
  - compile
  - 1회 evaluate
  - 예외 발생 시 정상 통과
  - 예외가 없으면 `Unexpected success`
로 판정

### 현재 출력 섹션

`TotalTest()`는 현재 다음을 출력한다.

- typed benchmark 요약 헤더
- compile 시간 표
  - double
  - long
  - bool
  - string
- 평가 시간 표
  - double
  - long
  - bool
  - string
- checksum/hash 비교 표
  - double
  - long
  - bool
  - string
- 오류 예상 검증 표
  - double
  - long
  - bool
  - string

### 현재 checksum 정책

Double 검증은 다음을 비교한다.

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

Long 검증은 다음을 비교한다.

- compiler checksum
- native checksum
- absolute difference
- match 여부

String 검증은 다음을 비교한다.

- compiler FNV-1a hash checksum
- native FNV-1a hash checksum
- 반복 실행 전체의 exact mismatch count
- match 여부

### 현재 테스트 목적

현재 테스트 화면은 단순한 microbenchmark 화면만은 아니다.

이제는 다음 역할도 같이 한다.

- parser regression check
- native C#와의 의미 일치 검증
- 예상 오류 수식 검증
- `double`, `long`, `bool`, `string` 4타입 compile/evaluate benchmark

## 현재 한계

현재 한계는 다음과 같다.

- `Layout`, `Product`만 부분 구현됨
- `Rider`, `Rate`, `Expense`, `VarChg`, `SInfo`, `ChkExprs` 로더는 미구현
- 수식 런타임은 아직 실제 업무 rule 실행과 분리된 상태
- `ExpressionContext`는 아직 테스트 중심의 고정 property bag
- 함수 호출 구조는 있지만 실제 함수군은 test 메서드 수준
- string 지원은 아직 의도적으로 좁다.
  - string literal
  - string `+` concat
  - 양쪽이 모두 string일 때만 `=`, `==`, `!=`, `<>`
- 객체 기반 수식 모델은 아직 없음
- legacy 계산 클래스와 아직 연결되지 않음

## 다음 방향

가장 자연스러운 다음 단계는 아래와 같다.

1. `ExpressionContext` 인식 함수 주입 구현 (`ProductNameContains`, `RiderNameContains` 등) — 명세서: `LLM_MD_FILES/ContextInjectionForFunctions_claude.md`
2. `ExpressionContext`에 `ProductName`, `RiderName` 프로퍼티 추가
3. 나머지 Excel 시트 로더 구현
4. `ExpressionContext`를 넘어서는 실제 업무 변수 바인딩 설계
5. 현재 concat/equality 중심의 string 지원을 어디까지 확장할지 결정
6. edge case 중심 테스트를 업무 규칙 중심 테스트로 확장
7. 로드된 rule 데이터와 런타임 평가 계층 연결
8. 이후 계산 파이프라인 연결

비고: `ExpressionContext`는 추후 리팩토링 시 `CommutationTable`로 이름 변경 예정.
