# 배열 루프 상한을 n으로 제한하는 최적화 명세

## 개요

`CompileDoubleArrayInto`의 루프가 현재 항상 `MAXSIZE(131)` 회 실행된다.
레거시 PVPlus의 기수표 계산은 `for (int t = 0; t <= n; t++)` 패턴을 따른다.
이번 수정은 이 패턴과 맞추는 것이 목적이다.

- 시작 인덱스: `0`
- 종료 인덱스: `n` 포함
- 총 계산 길이: `n + 1`

---

## 현재 구현 (`ExpressionCompiler.cs`)

```csharp
// CompileDoubleArrayInto — length가 컴파일 타임 상수
var length = Expression.Constant(CommutationTable.MAXSIZE);  // 131 고정

// Loop exit condition
Expression.IfThen(
    Expression.GreaterThanOrEqual(index, length),  // i >= 131
    Expression.Break(breakLabel))
```

루프 실행 횟수: **항상 131회 고정**.

---

## 변경 설계

### MAXSIZE 타입 변경 반영

`CommutationTable.MAXSIZE`는 `const long`으로 변경됨.

- `Expression.Constant(CommutationTable.MAXSIZE)` → `long` 상수
- `context.n`도 `long`
- 배열 인덱스 및 `requiredLength`는 `int` — `long → int` 변환 필요

### 핵심 아이디어

`length`를 컴파일 타임 상수에서 **런타임 계산 로컬 변수**로 교체한다.

```
requiredLength = (int)(context.n + 1L)
```

`requiredLength`는 `int` 타입 로컬 변수. 배열 인덱스가 `int`이므로.

### n 유효성 검증

배열 loop 전에 `context.n` 범위를 검증한다. 잘못된 n을 조용히 삼키지 않는다.

- `context.n < 0` → `ArgumentOutOfRangeException`
- `context.n >= CommutationTable.MAXSIZE` → `ArgumentOutOfRangeException`
- 메시지: `"context.n must be between 0 and MAXSIZE - 1."`

`context.n`과 `MAXSIZE` 모두 `long`이므로 직접 비교 가능.
유효성 검증 통과 후 `(int)(context.n + 1L)`은 안전하게 int 범위 안에 들어옴.

### 가드(Guard) 기준 변경

가드 기준도 `requiredLength`로 맞춘다.

```
이전: target.Length >= MAXSIZE (131)
변경: target.Length >= requiredLength (n+1)
```

배열이 MAXSIZE로 초기화되어 있으므로 실질적으로 실패할 일은 없으나,
설계 의미상 "실제 사용하는 길이만 보장하면 된다"가 더 정확하다.

메시지:
- `"target length is smaller than n + 1."`
- `"source '{name}' length is smaller than n + 1."`

### prologue 구성 순서

1. `context.n` 범위 검증 (`CreateNRangeGuard`)
2. `requiredLength = (int)(context.n + 1L)` 계산
3. target 배열 길이 guard (`requiredLength` 기준)
4. source 배열 hoist + 길이 guard (`requiredLength` 기준)
5. `index = 0`
6. loop 실행

### 구현 골격

```csharp
public static Action<CommutationTable, double[]> CompileDoubleArrayInto(string text)
{
    var syntax = ParseExpression(text);
    var targetParameter = Expression.Parameter(typeof(double[]), "target");
    var index = Expression.Variable(typeof(int), "i");
    var requiredLength = Expression.Variable(typeof(int), "requiredLength");
    var breakLabel = Expression.Label("LoopBreak");
    var referencedArrayProperties =
        new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);

    _ = BindSyntax(syntax, index, null, referencedArrayProperties);

    var hoistedArrayLocals =
        new Dictionary<string, ParameterExpression>(StringComparer.OrdinalIgnoreCase);
    var locals = new List<ParameterExpression> { index, requiredLength };
    var prologue = new List<Expression>();

    // 1. context.n 범위 검증
    prologue.Add(CreateNRangeGuard());

    // 2. requiredLength = (int)(context.n + 1L)
    var contextN = Expression.Property(_contextParameter, nameof(CommutationTable.n)); // long
    prologue.Add(Expression.Assign(
        requiredLength,
        Expression.Convert(
            Expression.Add(contextN, Expression.Constant(1L)),
            typeof(int))));

    // 3. target 배열 guard
    prologue.Add(CreateMinLengthGuard(targetParameter, requiredLength, "target length is smaller than n + 1."));

    foreach (var (_, property) in referencedArrayProperties)
    {
        var local = Expression.Variable(typeof(double[]), $"src_{property.Name}");
        hoistedArrayLocals[property.Name] = local;
        locals.Add(local);
        prologue.Add(Expression.Assign(local, Expression.Property(_contextParameter, property)));
        // 4. source 배열 guard
        prologue.Add(CreateMinLengthGuard(local, requiredLength, $"source '{property.Name}' length is smaller than n + 1."));
    }

    var bodyExpression = BindSyntax(syntax, index, hoistedArrayLocals, null);
    bodyExpression = ConvertReturnExpression(bodyExpression, typeof(double));

    var loop = Expression.Loop(
        Expression.Block(
            Expression.IfThen(
                Expression.GreaterThanOrEqual(index, requiredLength), // i >= n+1
                Expression.Break(breakLabel)),
            Expression.Assign(
                Expression.ArrayAccess(targetParameter, index),
                bodyExpression),
            Expression.PostIncrementAssign(index)),
        breakLabel);

    var blockExpressions = new List<Expression>(prologue)
    {
        Expression.Assign(index, Expression.Constant(0)),
        loop
    };

    var block = Expression.Block(locals, blockExpressions);

    return Expression.Lambda<Action<CommutationTable, double[]>>(
        block,
        _contextParameter,
        targetParameter).Compile();
}
```

### helper 추가

#### `CreateNRangeGuard`

`0 <= context.n < MAXSIZE` 검증. `ArgumentOutOfRangeException` 발생.

```csharp
private static Expression CreateNRangeGuard()
{
    // if (context.n < 0 || context.n >= MAXSIZE)
    //     throw new ArgumentOutOfRangeException("context.n must be between 0 and MAXSIZE - 1.")
}
```

#### `CreateMinLengthGuard` (기존 — `Expression` 인자 버전)

기존 `CreateMinLengthGuard(array, Expression length, string message)` 시그니처를 그대로 재사용.
`requiredLength`가 `ParameterExpression`이므로 `Expression` 파라미터에 바로 전달 가능.

### API 변화

없음. `CompileDoubleArrayInto` / `CompileDoubleArrayAssignment` 시그니처 동일.

---

## `t` 예약 식별자 내재화

배열 수식에서 `t`를 시점 인덱스로 참조할 수 있도록 컴파일러 내부에서 처리한다.
CommutationTable 프로퍼티 추가 없이 `BindSyntax` (또는 `CreatePropertyExpression`) 내에서 처리.

### 규칙

```
수식에서 "t" 참조 (대소문자 무시)
  → indexParameter != null (= 배열 수식 컨텍스트)
      → indexParameter 직접 반환 (int 타입)
  → indexParameter == null (= 스칼라 수식 컨텍스트)
      → InvalidOperationException("'t' is only valid inside array expressions.")
```

### 구현 위치

`CreatePropertyExpression` 또는 `BindSyntax` 내 property lookup 분기 시점.

```csharp
if (string.Equals(name, "t", StringComparison.OrdinalIgnoreCase))
{
    if (indexParameter is null)
        throw new InvalidOperationException("'t' is only valid inside array expressions.");
    return indexParameter; // Expression.Variable(typeof(int), "i")
}
```

### 타입 처리

- `t`의 타입은 `int`
- 수식에서 `t < 2`, `t < n` 등 비교 시:
  - `t < 2` — 리터럴 `2`는 `long`. int vs long → 기존 타입 승격 규칙 적용 (`t`를 `long`으로 변환)
  - `t < n` — `n`은 `long`. 동일하게 승격

---

## Tail 인덱스 처리

`n+1 .. MAXSIZE-1` 구간은 계산하지 않는다.

- fresh 배열이면 기본값 `0.0` 유지
- 기존 배열을 재사용하면 tail 값이 남을 수 있음
- **1차 구현에서 tail zero-fill 없음** (레거시 PVPlus와 동일)
- tail 처리가 필요해지면 별도 2차 수정에서 `ClearTail(target, requiredLength)` 정책 도입

---

## Edge Cases

| 케이스 | 동작 |
|---|---|
| `n = 0` | `requiredLength = 1`. index 0만 계산 |
| `n = 130` | `requiredLength = 131`. 0~130 전부 계산 |
| `n = 131` (= MAXSIZE) | `ArgumentOutOfRangeException` |
| `n < 0` | `ArgumentOutOfRangeException` |
| `t` 수식 참조 (배열) | `indexParameter` (int) 반환 |
| `t` 수식 참조 (스칼라) | `InvalidOperationException` |

---

## 수정 파일

- `Services/ExpressionCompiler.cs`
  - `CompileDoubleArrayInto`: `requiredLength` 로컬 변수 추가, n 검증 + 계산 prologue, guard 기준 변경, loop exit condition 교체
  - `CreateNRangeGuard` helper 추가
  - `CreatePropertyExpression` (또는 `BindSyntax`): `"t"` 예약 식별자 처리 추가
