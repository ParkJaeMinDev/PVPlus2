# ExpressionCompiler_ArrayLoopByN — `0..n` 루프 전환 및 `t` 내재화 명세

## 1. 목적

현재 `ExpressionCompiler.CompileDoubleArrayInto()`는 배열 계산 시 항상 `CommutationTable.MAXSIZE` 전체를 순회한다.

레거시 `reference_PVPlus`의 기수표 계산은 대체로 `t = 0 .. n` 구간만 계산하고, 그 바깥 구간은 기본값 상태로 남겨두는 방식이다.

이번 수정의 목표는 두 가지다.

- 배열 계산 범위를 `MAXSIZE 고정`에서 `context.n` 기반 `0..n`으로 바꾼다.
- 수식에서 사용하던 변수 `t`를 `CommutationTable` 프로퍼티가 아니라 **배열 모드 내부 루프 인덱스**로 내재화한다.

즉 최종적으로:

- 시작 index: `0`
- 종료 index: `n` 포함
- 총 계산 길이: `n + 1`
- `t` 식별자: 현재 loop index를 나타내는 예약 식별자

로 동작해야 한다.

---

## 2. 현재 동작

현 구현은 [ExpressionCompiler.cs](/C:/_z_work/PVPlus2/PVPlus2/Services/ExpressionCompiler.cs) 에서 다음과 같이 동작한다.

- `length = CommutationTable.MAXSIZE`
- `i = 0 .. MAXSIZE - 1`
- `target`, `source` 배열 길이도 `MAXSIZE` 이상을 요구

즉 `n` 값은 배열 loop에 반영되지 않는다.

또한 현재 `t`는 내부 예약 식별자가 아니므로, 수식에서 `t`를 쓰면 일반 프로퍼티 lookup 경로를 타게 된다.  
이번 수정 이후에는 `t`를 프로퍼티로 추가하지 않고, 배열 모드에서만 내부 식별자로 처리한다.

---

## 3. 목표 동작

배열 계산 길이는 `context.n`을 기준으로 runtime에 결정한다.

- `n = 0` 이면 `t = 0` 한 칸만 계산
- `n = 5` 이면 `t = 0, 1, 2, 3, 4, 5` 계산
- `n = 130` 이면 `t = 0 .. 130` 계산

즉 실제 loop 길이는:

```text
requiredLength = n + 1
```

이다.

`CommutationTable.MAXSIZE`는 현재 `const long`이므로, 허용 범위는 long 기준으로:

```text
0 <= n < MAXSIZE
```

이다.

---

## 4. 변경 대상

이번 수정의 직접 대상은 아래 세 곳이다.

- `CompileDoubleArrayInto(string text)`
- `CompileDoubleArrayAssignment(string targetPropertyName, string text)`
- `CreatePropertyExpression(...)` 또는 동등한 identifier binding 지점

핵심 변경은 `CompileDoubleArrayInto()`와 `t` 예약 식별자 처리에 집중된다.

스칼라 compile 경로는 이번 수정의 직접 대상이 아니지만, `t`를 잘못 사용할 때의 예외 동작은 명시해야 한다.

---

## 5. 상세 설계

### 5.1 `MAXSIZE`가 `long`인 점을 먼저 반영

`CommutationTable.MAXSIZE`는 현재 `const long`이다.  
하지만 expression tree에서 사용하는 아래 값들은 `int` 기반이다.

- 배열 인덱스 `index`
- `ArrayLength(...)`
- `requiredLength`

따라서 `ExpressionCompiler` 내부에서는 `MAXSIZE`를 두 가지 형태로 다뤄야 한다.

- `maxSizeLong`: `n` 범위 검증용
- `maxSizeInt`: 배열 길이 비교 및 loop 종료 조건용

권장 형태:

```csharp
var maxSizeLong = Expression.Constant(CommutationTable.MAXSIZE, typeof(long));
var maxSizeInt = Expression.Constant(checked((int)CommutationTable.MAXSIZE), typeof(int));
```

중요:

- `index >= MAXSIZE` 같은 비교에 `long` 상수를 직접 쓰면 타입이 맞지 않는다.
- 배열 길이/인덱스 관련 비교는 반드시 `int` 표현식끼리 맞춘다.

### 5.2 loop 길이 계산

기존:

```csharp
var length = Expression.Constant(CommutationTable.MAXSIZE);
```

변경:

- `context.n`을 읽는다.
- `n`의 범위를 먼저 long 기준으로 검증한다.
- 검증 후 `checked((int)context.n)`로 변환한다.
- 실제 loop 종료 조건은 `requiredLength = (int)context.n + 1`

권장 로컬 변수:

```csharp
var requiredLength = Expression.Variable(typeof(int), "requiredLength");
```

### 5.3 `n` 유효성 검증

배열 loop 전에 `context.n` 검증을 수행한다.

검증 조건:

- `context.n < 0` 이면 실패
- `context.n >= CommutationTable.MAXSIZE` 이면 실패

즉 `n`은 `0 .. MAXSIZE - 1` 범위만 허용된다.

권장 예외:

- `ArgumentOutOfRangeException`

권장 메시지:

```text
context.n must be between 0 and MAXSIZE - 1.
```

중요:

- 이번 수정에서는 `n`을 clamp하지 않는다.
- 잘못된 `n`을 조용히 보정하지 않고 명시적으로 실패시킨다.

### 5.4 source / target 길이 guard

기존:

- `target.Length >= MAXSIZE`
- `source.Length >= MAXSIZE`

변경:

- `target.Length >= requiredLength`
- `source.Length >= requiredLength`

즉 더 이상 `131`칸 전체를 강제하지 않고, 실제 계산에 필요한 길이만 검증한다.

권장 메시지:

- `target length is smaller than n + 1.`
- `source 'Rate_이율' length is smaller than n + 1.`

필요하면 실제 숫자를 메시지에 포함해도 되지만, 1차 구현에서는 고정 문자열로도 충분하다.

### 5.5 loop 종료 조건

기존:

```csharp
if (index >= CommutationTable.MAXSIZE) break;
```

변경:

```csharp
if (index >= requiredLength) break;
```

즉 loop는 `0 .. n`까지만 돈다.

### 5.6 `t` 예약 식별자 내재화

`t`는 `CommutationTable` 프로퍼티로 추가하지 않는다.  
배열 모드에서만 내부 루프 인덱스를 의미하는 예약 식별자로 처리한다.

권장 규칙:

- identifier 이름이 `"t"`이면 대소문자 무시 비교로 special-case 처리
- `indexParameter is not null`인 배열 모드에서는 현재 index를 반환
- `indexParameter is null`인 스칼라 모드에서는 예외 발생

권장 구현 위치:

- `CreatePropertyExpression(...)` 진입 직후
- 또는 property reflection 전에 수행되는 별도 identifier binding helper

권장 동작:

```csharp
if (string.Equals(name, "t", StringComparison.OrdinalIgnoreCase))
{
    if (indexParameter is null)
    {
        throw new InvalidOperationException("'t' is only valid inside array expressions.");
    }

    return Expression.Convert(indexParameter, typeof(long));
}
```

중요:

- 현재 컴파일러 숫자 체계는 `long` / `double`만 지원한다.
- 따라서 `t`는 `int` 그대로 반환하지 않고 `long`으로 변환해서 바인딩한다.
- 이렇게 해야 `t < 2`, `t + 1`, `if(t = 0, ...)` 같은 수식이 기존 숫자 규칙 안에서 자연스럽게 동작한다.

예시:

```text
if(t < 2, 0.99, 0.98)
```

이 식은 `n = 5`일 때:

```text
[0.99, 0.99, 0.98, 0.98, 0.98, 0.98]
```

를 생성해야 한다.

### 5.7 tail 인덱스 처리

`n + 1 .. MAXSIZE - 1` 구간은 이번 loop에서 계산하지 않는다.

이 구간 처리 정책은 레거시 PVPlus와 맞춘다.

- fresh array이면 기본값 `0`이 유지된다.
- 기존 배열을 재사용하면 tail 값이 남아 있을 수 있다.

이번 수정안의 기본 정책은:

- **tail을 자동으로 zero-fill 하지 않는다**

이다.

주의:

- 같은 target 배열을 재사용하면서 `n`이 줄어드는 경우, 이전 tail 값이 남을 수 있다.
- 이 동작이 문제되면 별도 2차 수정에서 `ClearTail(target, requiredLength)` 정책을 도입한다.

---

## 6. 권장 구현 형태

### 6.1 `CompileDoubleArrayInto()` 로컬 변수

권장 로컬 변수:

```csharp
var index = Expression.Variable(typeof(int), "i");
var requiredLength = Expression.Variable(typeof(int), "requiredLength");
```

`locals`에는 둘 다 포함한다.

### 6.2 prologue 구성

권장 순서:

1. `context.n`을 long으로 읽기
2. `n` 범위 검증
3. `requiredLength = checked((int)context.n) + 1`
4. target 길이 guard
5. source array hoist
6. source 길이 guard
7. `index = 0`
8. loop 실행

### 6.3 의사 코드

```csharp
public static Action<CommutationTable, double[]> CompileDoubleArrayInto(string text)
{
    var syntax = ParseExpression(text);
    var targetParameter = Expression.Parameter(typeof(double[]), "target");
    var index = Expression.Variable(typeof(int), "i");
    var requiredLength = Expression.Variable(typeof(int), "requiredLength");
    var breakLabel = Expression.Label("LoopBreak");

    var contextNLong = Expression.Property(_contextParameter, nameof(CommutationTable.n));
    var maxSizeLong = Expression.Constant(CommutationTable.MAXSIZE, typeof(long));
    var maxSizeInt = Expression.Constant(checked((int)CommutationTable.MAXSIZE), typeof(int));

    // pass 1
    _ = BindSyntax(syntax, index, null, referencedArrayProperties);

    // validate 0 <= context.n < MAXSIZE
    // requiredLength = checked((int)context.n) + 1
    // target/source length guard: >= requiredLength

    var bodyExpression = BindSyntax(syntax, index, hoistedArrayLocals, null);
    bodyExpression = ConvertReturnExpression(bodyExpression, typeof(double));

    var loop = Expression.Loop(
        Expression.Block(
            Expression.IfThen(
                Expression.GreaterThanOrEqual(index, requiredLength),
                Expression.Break(breakLabel)),
            Expression.Assign(
                Expression.ArrayAccess(targetParameter, index),
                bodyExpression),
            Expression.PostIncrementAssign(index)),
        breakLabel);

    ...
}
```

---

## 7. helper 추가 권장안

반복되는 검증식을 줄이기 위해 아래 helper 추가를 권장한다.

### 7.1 `CreateArrayRequiredLengthAssignment`

역할:

- `context.n`을 읽어 `requiredLength`를 계산

예시 개념:

```csharp
private static Expression CreateArrayRequiredLengthAssignment(
    ParameterExpression requiredLength)
```

### 7.2 `CreateNRangeGuard`

역할:

- `0 <= context.n < MAXSIZE` 검증

예시 개념:

```csharp
private static Expression CreateNRangeGuard()
```

### 7.3 `t` 예약 식별자 처리 helper

필수는 아니지만 `CreatePropertyExpression(...)` 복잡도가 커지면 아래 helper로 분리할 수 있다.

```csharp
private static bool TryBindInternalArrayIdentifier(
    string name,
    ParameterExpression? indexParameter,
    out Expression expression)
```

이 helper는 현재 시점에서는 `t`만 처리한다.

---

## 8. 테스트 영향

기존 `ExpressionArrayTests`는 대부분 그대로 유효하지만, 길이 guard 테스트는 기준이 바뀐다.

### 8.1 유지되는 테스트

- element-wise 산술
- array + scalar 혼합
- `if`
- `ifs`
- `CompileDoubleArrayAssignment`
- `float/single` cast 거부

### 8.2 수정이 필요한 테스트

기존 테스트는 `MAXSIZE - 1` 길이 target/source를 실패로 봤다.  
하지만 새 정책에서는 `n + 1`만 충족하면 성공할 수 있다.

따라서 길이 실패 테스트는 다음처럼 바꾼다.

- `context.n = 10`
- 필요한 길이 = `11`
- `target.Length = 10` -> 실패
- `source.Length = 10` -> 실패

### 8.3 신규 테스트 권장

- `n = 0`이면 index `0`만 계산되는지
- `n = 5`이면 `0..5`만 계산되고 `6` 이후는 untouched인지
- `n = 130`이면 `0..130` 전부 계산되는지
- `n = -1`이면 예외인지
- `n = 131`이면 예외인지
- `CompileDoubleArrayInto("t")`가 `0..n`을 기록하는지
- `CompileDoubleArrayInto("if(t < 2, 0.99, 0.98)")`가 기대 배열을 만드는지
- 스칼라 경로에서 `CompileDouble("t")`가 명시적으로 실패하는지

---

## 9. CommutationTable와의 정합성

레거시 reference PVPlus의 기수표 계산은 대체로 다음 패턴을 따른다.

- rate 계산 loop: `for (int t = 0; t <= n; t++)`
- 배열 크기: `MAXSIZE = 131`
- 의미 있는 값은 주로 `0..n`
- `t`는 프로퍼티가 아니라 loop 문맥에서 해석되는 값

이번 수정은 이 패턴과 맞추는 것이다.

즉:

- 배열 크기 정책은 그대로 `MAXSIZE`
- 계산 범위 정책은 `0..n`
- `t`는 `CommutationTable` 외부 프로퍼티가 아니라 내부 loop index

이다.

---

## 10. 결론

이번 수정안의 핵심은 두 가지다.

- **배열 계산의 실행 범위를 `MAXSIZE 고정`에서 `context.n 기반 0..n`으로 바꾼다.**
- **`t`를 `CommutationTable` 프로퍼티가 아닌 배열 모드 내부 예약 식별자로 내재화한다.**

그에 따라 함께 반영해야 하는 것은 아래 항목들이다.

- `MAXSIZE(long)`와 배열 인덱스/길이(`int`)의 타입 분리 처리
- `requiredLength = n + 1`
- `n` 범위 검증
- source/target 길이 guard 기준 변경
- loop break 조건 변경
- `t -> (long)indexParameter` 바인딩

tail zero-fill은 이번 1차 수정에 포함하지 않는다.
