# 컨텍스트 주입 함수 지원 명세서

## 1. 목표

`ExpressionFunctions.cs`의 메서드가 `ExpressionContext`의 필드(`ProductName`, `RiderName` 등)에
접근할 수 있도록, 첫 번째 파라미터가 `ExpressionContext`인 메서드를 바인더가 자동으로 감지하여
`_contextParameter`를 주입하는 방식을 지원한다.

수식 작성자는 컨텍스트 주입 여부를 의식하지 않고 기존 함수 호출 문법을 그대로 사용한다.

```
ProductNameContains("종신")   →   컴파일 후: context => ProductNameContains(context, "종신")
RiderNameContains("암")       →   컴파일 후: context => RiderNameContains(context, "암")
```

---

## 2. 배경

### 2.1 레거시 함수

레거시 PVPlus에서는 아래 함수들이 Flee ExpressionContext에 등록되어 사용됐다.

```csharp
public static bool ProductNameContains(string s)
    => PV.finder.FindProductRule().상품명.Contains(s);

public static bool RiderNameContains(string s)
    => PV.finder.FindRiderRule(lineInfo.RiderCode).담보명.Contains(s);
```

PVPlus2에서는 `상품명` → `ProductName`, `담보명` → `RiderName`으로 매핑되며,
이 값들은 `ExpressionContext`의 프로퍼티로 관리된다.

### 2.2 현재 구조의 한계

현재 `ExpressionFunctions`의 정적 메서드는 `ExpressionContext`에 접근할 방법이 없다.

- `ExpressionFunctions`는 순수 정적 클래스
- 컴파일된 람다의 `_contextParameter`는 `ExpressionCompiler` 내부에서만 관리됨
- 함수 등록 시점에 컨텍스트 인스턴스가 존재하지 않음

---

## 3. 핵심 원리

`_contextParameter`는 실행 시점의 값이 아닌 **Expression 트리의 노드**다.

현재도 프로퍼티 접근 시 동일한 방식으로 사용되고 있다.

```csharp
// 기존 — 프로퍼티 접근
Expression.Property(_contextParameter, property)
// → 람다 실행 시 context.x 를 읽는 트리 노드

// 이번 — 함수 인수로 주입
Expression.Call(method, _contextParameter, arg1)
// → 람다 실행 시 method(context, arg1) 을 호출하는 트리 노드
```

컴파일 결과:

```csharp
// 수식: ProductNameContains("종신")
// 생성되는 람다:
context => ProductNameContains(context, "종신")
```

`_contextParameter`는 람다 파라미터 선언과 본문에서 동일한 객체를 참조하기 때문에,
트리 어느 위치에 삽입해도 컴파일러가 올바르게 연결한다.

---

## 4. 변경 범위

| 파일 | 변경 내용 |
|---|---|
| `Services/ExpressionCompiler.cs` | `CreateReflectionFunctionCallExpression` 수정 — 컨텍스트 주입 감지 |
| `Services/ExpressionFunctions.cs` | 컨텍스트 의존 함수 추가 |
| `Models/ExpressionContext.cs` | `ProductName`, `RiderName` 프로퍼티 추가 |

파서 변경 없음.

---

## 5. 컨텍스트 주입 감지 규칙

바인더가 후보 메서드를 처리할 때 아래 조건을 확인한다.

```
첫 번째 파라미터의 타입 == typeof(ExpressionContext)
```

조건이 맞으면 **컨텍스트 주입 메서드**로 분류하고 아래 규칙을 적용한다.

| 항목 | 일반 메서드 | 컨텍스트 주입 메서드 |
|---|---|---|
| 인수 수 매칭 | `parameters.Length == arguments.Count` | `parameters.Length - 1 == arguments.Count` |
| 첫 번째 인수 | 수식에서 제공 | `_contextParameter` 자동 주입 |
| 나머지 인수 | 기존 방식 | 기존 방식 |
| score 계산 | 기존 방식 | 기존 방식 (주입 파라미터 제외) |

컨텍스트 주입 메서드에는 **penalty 없음**.
`_contextParameter` 주입은 변환이 아니라 항등 연결이므로 score에 영향을 주지 않는다.

---

## 6. 알고리즘 상세

### 6.1 감지 및 분기

```
후보 메서드 순회:
    isContextInjected = parameters.Length > 0
                        && parameters[0].ParameterType == typeof(ExpressionContext)

    isContextInjected == true  → 컨텍스트 주입 경로 (6.2)
    isContextInjected == false → 기존 경로 (고정 arity / params)
```

### 6.2 컨텍스트 주입 경로

```
effectiveParameterCount = parameters.Length - 1

if arguments.Count != effectiveParameterCount → skip

convertedArguments = new Expression[parameters.Length]
convertedArguments[0] = _contextParameter   // 자동 주입
score = 0

for i in 1..parameters.Length-1:
    TryConvertFunctionArgument(arguments[i-1], parameters[i].ParameterType, ...)
    실패 시 → skip
    성공 시 → score 누적

최종 호출: Expression.Call(method, convertedArguments)
```

### 6.3 params와의 조합

컨텍스트 주입 + params 조합도 지원 가능하다.
첫 파라미터가 `ExpressionContext`이고 마지막 파라미터가 `params T[]`인 경우:

```csharp
public static double SomeFunc(ExpressionContext context, string label, params double[] values)
```

처리 순서:
1. 컨텍스트 주입 감지 → `_contextParameter` 주입
2. 나머지 파라미터에 대해 params 매칭 적용

단, 이 조합은 현재 필요가 확인된 경우에만 구현한다.
지금 당장은 컨텍스트 주입 단독(고정 arity)만 구현해도 충분하다.

---

## 7. ExpressionFunctions.cs 작성 규칙

컨텍스트 의존 함수는 첫 번째 파라미터를 반드시 `ExpressionContext context`로 선언한다.

```csharp
// 컨텍스트 의존 함수 — 첫 파라미터로 ExpressionContext 선언
public static bool ProductNameContains(ExpressionContext context, string s)
    => context.ProductName.Contains(s);

public static bool RiderNameContains(ExpressionContext context, string s)
    => context.RiderName.Contains(s);
```

수식에서는 컨텍스트 파라미터 없이 호출한다.

```
ProductNameContains("종신")
RiderNameContains("암")
```

### 7.1 일반 함수와 혼용 금지

같은 이름에 컨텍스트 주입 버전과 비주입 버전을 동시에 두지 않는다.

```csharp
// 금지
public static bool ProductNameContains(string s) => ...          // 비주입
public static bool ProductNameContains(ExpressionContext ctx, string s) => ...  // 주입
```

바인더가 동점 처리로 ambiguous 예외를 던질 수 있다.

---

## 8. ExpressionContext.cs 변경사항

`ProductName`, `RiderName` 프로퍼티를 추가한다.

```csharp
public string ProductName { get; set; } = string.Empty;
public string RiderName { get; set; } = string.Empty;
```

---

## 9. 테스트 케이스

### 9.1 컨텍스트 주입 성공

| 수식 | 컨텍스트 상태 | 예상 결과 |
|---|---|---|
| `ProductNameContains("종신")` | `ProductName = "종신보험"` | `true` |
| `ProductNameContains("화재")` | `ProductName = "종신보험"` | `false` |
| `RiderNameContains("암")` | `RiderName = "암진단특약"` | `true` |
| `ProductNameContains("종신") AND RiderNameContains("암")` | 둘 다 포함 | `true` |

### 9.2 if/ifs와의 조합

| 수식 | 예상 동작 |
|---|---|
| `if(ProductNameContains("종신"), 1.0, 0.0)` | 정상 동작 |
| `ifs(ProductNameContains("종신"), 1.0, RiderNameContains("암"), 2.0, 0.0)` | 정상 동작 |

### 9.3 기존 함수 회귀

컨텍스트 주입 감지 로직 추가 후에도 기존 함수 동작이 변하지 않아야 한다.

| 수식 | 기대 동작 |
|---|---|
| `Min(1, 2, 3)` | 정상 동작 |
| `Abs(-1.5)` | 정상 동작 |
| `cast(1.9, int)` | 정상 동작 |
| `if(a > 0, 1, 0)` | 정상 동작 |

---

## 10. 비고 및 제약

- 컨텍스트 주입은 **첫 번째 파라미터 한 개**에만 적용한다. 두 번째 이후 파라미터에 `ExpressionContext`를 쓰는 것은 지원하지 않는다.
- `ExpressionContext` 타입 감지는 정확한 타입 일치(`==`)로 판별한다. 서브클래스는 해당 없음.
- `ProductName`, `RiderName`이 `null`인 경우 `string.Contains`에서 `NullReferenceException`이 발생할 수 있다. 초기값을 `string.Empty`로 설정하여 방지한다.
- 나중에 `ExpressionContext`가 `CommutationTable`로 이름이 바뀌면 바인더의 타입 감지 조건만 수정하면 된다.
