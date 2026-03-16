# Flee 연산자 및 함수 정리

기준 저장소: `mparlak/Flee`

이 문서는 **Flee의 언어 사양(문서 기준)** 과 **GitHub 포트에서 보고된 차이/버그** 를 구분해서 정리한 문서다.
대체 평가기를 구현할 때는 보통 **언어 사양** 을 먼저 따라가고, 포트 버그는 호환성 여부를 별도로 결정하는 편이 안전하다.

---

## 1. 먼저 결론: Flee에 "고정된 거대한 내장 함수 목록"이 있는가?

엄밀히 말하면 **그렇지 않다**.

Flee에서 식 안에서 호출 가능한 함수는 크게 4종류다.

1. **언어 차원의 특수 함수 / 특수 구문**
   - `if(condition, whenTrue, whenFalse)`
   - `cast(value, type)`

2. **Imports로 추가한 타입의 public static 메서드**
   - 예: `context.Imports.AddType(typeof(Math))`
   - 그러면 `sqrt`, `cos`, `max` 같은 `System.Math`의 public static 멤버를 함수처럼 호출 가능

3. **변수의 public instance 메서드**
   - 변수는 자신의 타입 인스턴스처럼 동작하므로 instance 메서드 호출 가능
   - 예: `rand.nextDouble()`

4. **expression owner의 메서드**
   - owner를 붙이면 owner의 static / instance 메서드 사용 가능

즉, Flee 자체가 NCalc처럼 "미리 박혀 있는 수십 개의 고정 내장 함수 세트"를 제공하는 구조라기보다,
**언어 핵심은 연산자/리터럴/특수구문이고, 일반 함수 호출은 import/variable/owner를 통해 확장되는 구조** 다.

---

## 2. 언어 기본 규칙

- **대소문자를 구분하지 않음**
  - 예: `if`, `If`, `IF` 모두 같은 취급
- 문법 성격은 **C# + VB.Net 혼합형**
- **강한 타입 시스템**을 사용함
- late binding 없음

---

## 3. 연산자 / 문법 요소 전체 목록

아래는 문서와 토큰/파서 흔적을 바탕으로 정리한 **Flee 언어 차원의 연산자 및 핵심 문법 요소 전체 목록** 이다.

### 3.1 산술 연산자

- `+` : 덧셈 / 문자열 결합
- `-` : 뺄셈
- `*` : 곱셈
- `/` : 나눗셈
- `%` : 나머지
- `^` : 거듭제곱
- 단항 `+`
- 단항 `-` (negation)

### 3.2 비교 연산자

- `=` : 같음
- `<>` : 다름
- `<` : 미만
- `<=` : 이하
- `>` : 초과
- `>=` : 이상

### 3.3 논리 / 비트 연산자

Flee는 같은 키워드를 **bool에서는 논리 연산**, **정수형에서는 비트 연산** 으로 사용한다.

- `And`
- `Or`
- `Xor`
- `Not`

주의:
- 두 피연산자가 모두 `bool`이면 논리 연산
- 두 피연산자가 모두 정수형이면 비트 연산
- 그 외 조합은 컴파일 오류

### 3.4 시프트 연산자

- `<<`
- `>>`

정수형에만 유효하다.

### 3.5 문자열 결합

- `+`

한쪽이라도 문자열이면 문자열 결합으로 동작한다.

### 3.6 멤버 접근 / 호출 / 인덱싱

- `.` : 멤버 접근
  - 예: `obj.Prop`, `obj.Method()`
- `()` : 함수/메서드 호출
- `[]` : 인덱서 / 배열 / 컬렉션 접근
  - 예: `arr[i + 1]`

### 3.7 조건 연산(특수 함수 형태)

- `if(condition, whenTrue, whenFalse)`

문서상으로는 **진짜 short-circuit conditional** 이다.
즉, 조건에 맞는 분기만 평가한다.

### 3.8 캐스팅(특수 함수 형태)

- `cast(value, type)`

예:
- `cast(obj, int)`

### 3.9 포함 여부 연산자

- `In`

두 가지 형태가 있다.

1. **리스트 검색**
   - `value in (value1, value2, value3, ...)`

2. **컬렉션 검색**
   - `value in collection`

문서상 컬렉션은 다음 중 하나를 구현해야 한다.
- `ICollection<T>`
- `IDictionary<K,V>`
- `IList`
- `IDictionary`
- 배열도 가능

---

## 4. 리터럴 전체 목록

### 4.1 문자
- `'a'`

### 4.2 불리언
- `true`
- `false`

### 4.3 실수 리터럴
- 소수점이 있으면 실수 취급
- 구 문서 기준:
  - 기본은 `double`
  - `f` suffix로 `float` 강제
- 이후 문서 개정본 기준:
  - `d` = double
  - `f` = float/single
  - `m` = decimal
  - suffix가 없을 때는 `ExpressionOptions.RealLiteralDataType` 설정값 사용 가능

### 4.4 정수 리터럴
- 소수점이 없으면 정수 취급
- `L` suffix: 64비트 정수 강제
- `U` suffix: unsigned 강제
- 값이 들어가는 가장 작은 적절한 정수형으로 배치 시도

### 4.5 16진수 리터럴
- 예: `0xFF12`

### 4.6 문자열 리터럴
- `"text"`
- 이스케이프 규칙은 C#과 동일

### 4.7 null 리터럴
- `null`

### 4.8 DateTime 리터럴
- `#08/06/2008#`
- 포맷은 `ExpressionOptions.DateTimeFormat`으로 제어 가능

### 4.9 TimeSpan 리터럴
- `##[d.]hh:mm[:ss[.ff]]#`
- 예: `##1.23:45#`

---

## 5. 함수 호출 관련 정리

## 5.1 Flee가 언어 차원에서 특별 취급하는 함수

문서 기준으로 특별 취급되는 것은 사실상 아래 2개다.

1. `if(condition, whenTrue, whenFalse)`
2. `cast(value, type)`

이 둘은 일반 import 함수라기보다 **언어 문법 요소에 가까운 특수 함수** 다.

---

## 5.2 Imports로 들어오는 함수

### 방식
```csharp
ExpressionContext context = new ExpressionContext();
context.Imports.AddType(typeof(Math));
```

그러면 해당 타입의 **public static 메서드** 를 식에서 직접 함수처럼 호출할 수 있다.

예:
```csharp
sqrt(a) + pi
cos(a)
max(1.23, 4.56)
```

### 중요한 점
이 함수들은 **Flee 자체 내장 함수** 라기보다,
**네가 import한 타입의 public static 멤버** 다.

즉 `System.Math`를 import하면 사용할 수 있는 함수 목록은 사실상 **해당 런타임 버전의 `System.Math` public static 멤버 목록 전체** 에 의해 결정된다.
따라서 Flee 자체 저장소만 보고 "Math 함수 전체를 하나도 빠짐없이 고정 목록으로" 적는 것은 정확하지 않다.

실제로 Flee 문서도 `Math`를 예로 들 뿐, Flee 내부 고정 함수 테이블을 제시하지 않는다.

---

## 5.3 변수의 인스턴스 메서드 호출

변수는 자신의 타입 인스턴스처럼 동작한다.

예:
```csharp
context.Variables.Add("rand", new Random());
rand.nextDouble()
```

즉 변수 타입의 **public instance 메서드** 사용 가능.

---

## 5.4 Expression owner 메서드 호출

owner를 붙이면 owner의 메서드를 식에서 직접 호출할 수 있다.

예:
```csharp
ExpressionContext context = new ExpressionContext(rand);
nextDouble() + 100
```

문서상 owner에서는 public / non-public, static / instance 멤버 접근이 가능하다.
(실제 접근 범위는 옵션으로 제어)

---

## 5.5 `ImportBuiltinTypes()` 는 무엇인가?

구형 XML 문서 설명에는 `ImportBuiltinTypes()` 가
**built-in types (예: `int`, `string`, `double`)를 expression에 import** 해 준다고 되어 있다.

즉 이것은 보통:
- `cast(obj, int)` 같은 표현에서 타입 이름을 쓰기 쉽게 하거나
- 기본 타입의 멤버를 식에서 참조할 수 있게 하는 용도

로 이해하는 것이 맞다.

다만 공개 문서에서 확인되는 스니펫은 예시로 `int`, `string`, `double`만 보여 주며,
내가 이번 조사에서 **Flee GitHub 저장소 기준으로 builtin type alias 전체를 1:1 완전 목록으로 검증한 자료는 찾지 못했다.**
그래서 여기서는 예시로만 적고, "완전 목록"이라고 단정하지 않는다.

---

## 6. 파서/구문 구현 시 바로 참고할 수 있는 체크리스트

대체 구현에서 최소 호환 목표를 잡으려면 아래를 지원하면 된다.

### 핵심 연산자
- 산술: `+ - * / % ^`
- 비교: `= <> < <= > >=`
- 논리/비트: `And Or Xor Not`
- 시프트: `<< >>`
- 포함: `In`
- 문자열 결합: `+`
- 인덱싱: `[]`
- 멤버 접근: `.`
- 호출: `()`
- 특수 함수: `if`, `cast`

### 리터럴
- char
- bool
- real
- integral
- hex
- string
- null
- datetime(`#...#`)
- timespan(`##...#`)

### 함수 해석 순서(권장)
1. 특수 함수 `if`, `cast` 우선
2. owner 멤버
3. imported static 함수
4. 변수 instance 메서드
5. 필요시 on-demand function hook

---

## 7. GitHub 포트에서 보고된 차이 / 버그

문서상 지원과 별개로, `mparlak/Flee` GitHub 포트 이슈에서는 아래와 같은 사례가 보고됐다.

### 7.1 `null` 리터럴 문제
- 문서상 `null` 지원
- 하지만 GitHub 포트 이슈에서는 `someType <> null` 이
  `null` 을 리터럴이 아니라 식별자로 해석하는 문제가 보고됨

### 7.2 `if(...)`, `cast(...)` 문제
- 문서상 둘 다 특수 구문/특수 함수로 설명됨
- 하지만 GitHub 포트 이슈에서는 `if(a>b,a,b)` 가 일반 함수 호출처럼 처리되어
  함수 미정의 오류가 나는 사례가 보고됨

### 7.3 문화권(culture)별 구분자 문제
- 실수 소수점에 `,` 를 쓰는 culture에서는 함수 인자 구분자가 기본적으로 `;` 로 바뀔 수 있음
- 예: `max(1,23; 4,56)`

즉, **문서 사양과 GitHub 포트의 실제 동작이 항상 일치한다고 가정하면 안 된다.**
대체 구현을 만들 때는 “원래 언어 사양을 따라갈지”, “GitHub 포트의 실제 버그까지 호환할지”를 먼저 정하는 것이 좋다.

---

## 8. 구현 관점에서의 요약

Flee를 대체하는 evaluator를 만든다면, 사실상 아래처럼 생각하면 된다.

- **언어 핵심**
  - 연산자
  - 리터럴
  - 멤버 접근 / 호출 / 인덱싱
  - `if`, `cast`, `in`

- **함수 시스템 핵심**
  - 고정 내장 함수 테이블보다는
  - `imported static methods + variable instance methods + owner methods`

즉 Flee 호환의 핵심은
"내장 함수 100개를 복제"가 아니라,
**문법 + 타입체크 + import 기반 함수 바인딩 모델** 을 재현하는 데 있다.

---

## 9. 출처

- GitHub README: mparlak/Flee
- Flee Wiki mirror (Language Reference / Importing Types / Customizing Parser)
- GitHub Issues:
  - #6 null check is broken
  - #22 If and cast not working
  - #24 Flee fails to parse if formula
- Legacy XML docs snippet for `ImportBuiltinTypes()`
