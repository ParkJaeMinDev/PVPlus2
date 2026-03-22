# HelperFunctionsExpansion — ExpressionFunctions 확장 명세

레거시 `helper.cs` 기반으로 `ExpressionFunctions.cs`에 추가할 함수 목록과 구현 규칙을 정의한다.

---

## 1. 개요

레거시 PVPlus의 `helper.cs`에 정의된 함수 중 현재 타입 시스템(`long`, `double`, `bool`, `string`)과
CommutationTable 주입 패턴으로 구현 가능한 함수를 선별하여 추가한다.

### 타입 매핑 원칙
- 레거시 `int` 파라미터 → `long` (PVPlus2 정수 타입)
- 내부에서 실제 `int` 필요 시 `checked((int)value)` 변환
- 레거시 `double` 파라미터 → `double` 유지
- 레거시 `string` 파라미터 → `string` 유지

---

## 2. 이번 릴리스에서 추가할 함수

### 2.1 RoundUp / RoundDown

레거시의 자릿수 지정 올림/내림 함수.

| 시그니처 | 반환 | 비고 |
|---|---|---|
| `RoundUp(double number, long digits)` | `double` | 올림 |
| `RoundDown(double number, long digits)` | `double` | 내림 |

**동작 정의**

```
RoundUp(x, d)  = Math.Ceiling(x * 10^d) / 10^d
RoundDown(x, d) = Math.Floor(x  * 10^d) / 10^d
```

- `digits = 0`: 1원 단위 (정수)
- `digits = 2`: 소수점 2자리 단위
- `digits = -2`: 100 단위

**구현 예시**

```csharp
public static double RoundUp(double number, long digits)
{
    double n = Math.Pow(10.0, digits);
    return Math.Ceiling(number * n) / n;
}

public static double RoundDown(double number, long digits)
{
    double n = Math.Pow(10.0, digits);
    return Math.Floor(number * n) / n;
}
```

**경계 동작**
- `digits`가 큰 음수일 때 (`-15` 이하): `Math.Pow(10, digits)`가 0에 가까워져 결과 불안정 → CLR에 위임
- `number = 0`: `0.0` 반환
- `number = NaN`: `NaN` 반환 → CLR에 위임

---

### 2.2 Round2

레거시에서 부동소수점 머신 오류를 방지하기 위해 사용한 반올림 함수.
일반 `Round`와 달리 소수점 10자리 선처리를 먼저 수행하여 미세 오차를 제거한다.

| 시그니처 | 반환 | 비고 |
|---|---|---|
| `Round2(double number, long digits)` | `double` | 머신 오류 방지 반올림 |
| `Round2(double number)` | `double` | digits = 0 단축형 |

**동작 정의**

```
1) n = Round(number, 10, AwayFromZero)   // 10자리 선처리
2) if (n == number) n = Round(number, 9) // 동일하면 9자리로 (AwayFromZero 없이)
3) n = Round(n, digits, AwayFromZero)    // 최종 반올림
```

레거시 코드의 원본 로직을 그대로 유지한다.

**구현 예시**

```csharp
public static double Round2(double number, long digits)
{
    double n = Math.Round(number, 10, MidpointRounding.AwayFromZero);
    if (n == number) n = Math.Round(number, 9);
    return Math.Round(n, checked((int)digits), MidpointRounding.AwayFromZero);
}

public static double Round2(double number) => Round2(number, 0);
```

**레거시 대비 변경**
- `int digt` → `long digits`

---

### 2.3 PositiveMin / PositiveMax

양수 값만 대상으로 Min/Max를 구한다.
양수가 하나도 없으면 0을 반환한다.

| 시그니처 | 반환 | 비고 |
|---|---|---|
| `PositiveMin(params double[] values)` | `double` | 양수 중 최솟값 |
| `PositiveMax(params double[] values)` | `double` | 양수 중 최댓값 |

**구현 예시**

```csharp
public static double PositiveMin(params double[] values)
{
    double[] positive = values.Where(x => x > 0).ToArray();
    return positive.Length > 0 ? positive.Min() : 0.0;
}

public static double PositiveMax(params double[] values)
{
    double[] positive = values.Where(x => x > 0).ToArray();
    return positive.Length > 0 ? positive.Max() : 0.0;
}
```

**경계 동작**
- 인수가 모두 음수 또는 0: `0.0` 반환
- 인수 1개 이상: 정상 동작 (파서 레벨에서 `fn()` 0인수 호출 불가)

---

### 2.4 Average

가변 인수의 산술 평균.

| 시그니처 | 반환 |
|---|---|
| `Average(params double[] values)` | `double` |

**구현 예시**

```csharp
public static double Average(params double[] values) => values.Average();
```

---

### 2.5 Choose

1-based 인덱스로 인수 목록에서 값을 선택한다.
인덱스가 범위를 벗어나면 클램핑(1~Length)하여 반환한다.

| 시그니처 | 반환 | 비고 |
|---|---|---|
| `Choose(long index, params double[] items)` | `double` | 숫자 선택 |
| `Choose(long index, params long[] items)` | `long` | 정수 선택 |
| `Choose(long index, params string[] items)` | `string` | 문자열 선택 |

**동작 정의**

```
idx = clamp(index, 1, items.Length)
return items[idx - 1]
```

**구현 예시**

```csharp
public static double Choose(long index, params double[] items)
{
    int idx = Math.Max(Math.Min(checked((int)index), items.Length), 1);
    return items[idx - 1];
}

public static long Choose(long index, params long[] items)
{
    int idx = Math.Max(Math.Min(checked((int)index), items.Length), 1);
    return items[idx - 1];
}

public static string Choose(long index, params string[] items)
{
    int idx = Math.Max(Math.Min(checked((int)index), items.Length), 1);
    return items[idx - 1];
}
```

**레거시 대비 변경**
- `int index` → `long index`
- `Choose(int, params int[])` → `Choose(long, params long[])` (long 등가 오버로드로 이관)

**경계 동작**
- `index <= 0`: 첫 번째 항목 반환
- `index > items.Length`: 마지막 항목 반환

---

### 2.6 Left / Right / Mid

문자열에서 부분 문자열을 추출한다.

| 시그니처 | 반환 | 비고 |
|---|---|---|
| `Left(string s, long count)` | `string` | 왼쪽 count자 |
| `Right(string s, long count)` | `string` | 오른쪽 count자 |
| `Mid(string s, long start, long count)` | `string` | start(1-based)부터 count자 |

**구현 예시**

```csharp
public static string Left(string s, long count)
    => s.Substring(0, checked((int)count));

public static string Right(string s, long count)
    => s.Substring(s.Length - checked((int)count), checked((int)count));

public static string Mid(string s, long start, long count)
    => s.Substring(checked((int)start) - 1, checked((int)count));
```

**레거시 대비 변경**
- `int count` → `long count`
- `int item` / `double item` 오버로드 생략: 레거시에서 int/double에 `.ToString()`을 적용하는 방식이었으나 PVPlus2에서는 명시적 `cast(value, string)` 후 사용하는 것으로 대체

**경계 동작**
- `count` 또는 `start`가 범위를 벗어나면 `ArgumentOutOfRangeException` 발생 → CLR에 위임
- `s == null`: `NullReferenceException` 발생 → CLR에 위임 (ExpressionContext 주입 없음, API 계약 없음)

---

### 2.7 IndexOf

값 목록에서 항목을 찾아 1-based 위치를 반환한다. 없으면 -1.

| 시그니처 | 반환 | 비고 |
|---|---|---|
| `IndexOf(string item, params string[] items)` | `long` | 문자열 검색 |
| `IndexOf(long item, params long[] items)` | `long` | 정수 검색 |
| `IndexOf(double item, params double[] items)` | `long` | 실수 검색 |

레거시는 `object` 기반 단일 오버로드였으나, PVPlus2 타입 시스템에서 `object`는 지원하지 않으므로 타입별 오버로드로 분리한다.

**구현 예시**

```csharp
public static long IndexOf(string item, params string[] items)
{
    for (int i = 0; i < items.Length; i++)
        if (items[i] == item) return i + 1;
    return -1;
}

public static long IndexOf(long item, params long[] items)
{
    for (int i = 0; i < items.Length; i++)
        if (items[i] == item) return i + 1;
    return -1;
}

public static long IndexOf(double item, params double[] items)
{
    for (int i = 0; i < items.Length; i++)
        if (items[i] == item) return i + 1;
    return -1;
}
```

**레거시 대비 변경**
- `object` → 타입별 오버로드 3종
- `ToString()` 비교 → 직접 값 비교 (`==`)
- 반환 타입 `int` → `long`

**string 비교 정책**
- `StringComparison` 없이 `==` 사용: 기본값 `Ordinal` 동작과 동일 (레거시도 `.ToString() ==` 비교)

---

## 3. 제외 대상 및 이유

| 함수 | 제외 이유 |
|---|---|
| `Ifs` | ExpressionCompiler에 내장 특수 함수로 이미 구현 |
| `ToInt`, `ToDouble`, `ToString` | `cast()` 내장 함수로 대체 |
| `RoundA` | CommutationTable에 `Amount` 필드 추가 필요 — 별도 릴리스 |
| `D`, `U` | CommutationTable에 `t`, `S1` 변수 필요 — 별도 릴리스 |
| `S` | CommutationTable에 `Substandard_Mode` 필요 — 별도 릴리스 |
| `AgeSign` | CommutationTable에 `Age` 필요 — 별도 릴리스 |
| `Renewal` | CommutationTable에 `S1` 필요 — 별도 릴리스 |
| `Ax`, `Xx`, `V`, `W` | PVCalculator 의존 — 현 단계 불가 |
| `Pr`, `PrTerm`, `GP` | PVCalculator 의존 — 현 단계 불가 |
| `EVal`, `Ex`, `FindQ` | PVCalculator / 외부 데이터 의존 — 현 단계 불가 |
| `TypeOf`, `ThrowError` | 디버그용, 수식 API로 노출 불필요 |
| `ToIntOrDefault`, `ToDoubleOrDefault` | 수식 내 예외 처리 패턴 미지원 |
| `ToInt(item, items)` 등 매핑 오버로드 | 복잡한 문자열 파싱 로직 — 현 단계 불필요 |

---

## 4. 구현 위치

`Services/ExpressionFunctions.cs` 에 추가한다.
기존 함수 아래 섹션별로 배치:

```
// --- 자릿수 반올림 ---
RoundUp, RoundDown, Round2

// --- 집계 ---
PositiveMin, PositiveMax, Average

// --- 선택 ---
Choose

// --- 문자열 ---
Left, Right, Mid, IndexOf
```

---

## 5. 테스트 케이스

### 5.1 RoundUp / RoundDown

```
RoundUp(1.234, 2)    → 1.24
RoundUp(1.230, 2)    → 1.23
RoundDown(1.239, 2)  → 1.23
RoundUp(123.0, -2)   → 200.0
RoundDown(199.0, -2) → 100.0
RoundUp(0.0, 0)      → 0.0
```

### 5.2 Round2

```
Round2(1.5, 0)       → 2.0    // AwayFromZero
Round2(2.5, 0)       → 3.0    // AwayFromZero
Round2(1.2345, 2)    → 1.23
// 머신 오류 방지: 0.1 + 0.2 = 0.30000000000000004
Round2(0.1 + 0.2, 1) → 0.3
Round2(1.5)          → 2.0    // digits=0 단축형
```

### 5.3 PositiveMin / PositiveMax

```
PositiveMin(3, 1, 2)     → 1.0
PositiveMin(-1, -2, 0)   → 0.0   // 양수 없음
PositiveMax(3, 1, 2)     → 3.0
PositiveMax(-1, -2, -3)  → 0.0   // 양수 없음
```

### 5.4 Average

```
Average(1, 2, 3) → 2.0
Average(0, 10)   → 5.0
```

### 5.5 Choose

```
Choose(1, 10, 20, 30)    → 10.0        // double 오버로드
Choose(2, 10, 20, 30)    → 20.0
Choose(0, 10, 20, 30)    → 10.0        // clamp 하한
Choose(5, 10, 20, 30)    → 30.0        // clamp 상한
Choose(2, "a", "b", "c") → "b"
Choose(2, 10, 20, 30)    → 20          // long 오버로드 (정수 리터럴 → long 자동)
Choose(0, 10, 20, 30)    → 10          // long, clamp 하한
```

### 5.6 Left / Right / Mid

```
Left("abcde", 3)      → "abc"
Right("abcde", 3)     → "cde"
Mid("abcde", 2, 3)    → "bcd"   // 1-based, start=2
Mid("abcde", 1, 5)    → "abcde"
```

### 5.7 IndexOf

```
IndexOf("b", "a", "b", "c") → 2
IndexOf("z", "a", "b", "c") → -1
IndexOf(2, 1, 2, 3)         → 2
IndexOf(5.0, 1.0, 5.0, 3.0) → 2
```

### 5.8 컴파일 실패 케이스

```
// 타입 불일치 → 컴파일 오류
RoundUp("text", 2)
Left(123, 3)

// 인수 개수 불일치 → 컴파일 오류
RoundUp(1.5)         // digits 누락
Mid("abc", 1)        // count 누락

// 0인수 → 파서 오류
Average()
PositiveMin()
```

---

## 6. 미구현으로 남기는 함수 (향후 CommutationTable 필드 확장 시)

CommutationTable에 해당 필드가 추가되면 컨텍스트 주입 방식으로 구현 가능:

| 함수 | 필요 필드 |
|---|---|
| `RoundA(double number)` | `context.Amount` |
| `AgeSign(long t)` | `context.Age` |
| `S(double K)` | `context.Substandard_Mode` |
| `D(params double[] items)` | `context.t`, `context.S1` |
| `U(params double[] items)` | `context.t`, `context.S1`, `context.Age` |
