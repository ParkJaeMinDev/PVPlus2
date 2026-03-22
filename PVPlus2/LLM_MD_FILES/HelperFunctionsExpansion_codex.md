# HelperFunctionsExpansion_codex

`reference_PVPlus/helper.cs`의 함수 중 현재 `PVPlus2/Services/ExpressionFunctions.cs`로 이관 가능한 대상을 선별한 명세서다.

기준은 "현재 PVPlus2 식 엔진 구조를 거의 바꾸지 않고 추가 가능한가"이다. 단순히 helper에 존재한다는 이유만으로 옮기지 않고, 현재 타입 시스템, `CommutationTable`, 함수 바인딩 규칙, 외부 의존성을 함께 본다.

---

## 1. 현재 기준 제약

### 1.1 타입 시스템
- 식 엔진의 실질 타입은 `long`, `double`, `bool`, `string` 4종이다.
- 레거시 helper의 `int`는 PVPlus2에서는 `long`으로 매핑하는 것이 기본이다.
- `Substring`, 배열 인덱스처럼 CLR `int`가 필요한 지점만 `checked((int)value)`로 변환한다.

### 1.2 함수 바인딩 규칙
- `ExpressionFunctions`의 public static 메서드는 reflection으로 노출된다.
- 첫 번째 인자가 `CommutationTable`이면 context-injected 함수로 호출 가능하다.
- `params` 함수는 지원된다.
- 현재 컴파일러는 `context + params` 조합을 지원하지 않는다.
  - 따라서 `D(params double[])`, `U(params double[])` 같은 helper 스타일은 현재 구조 그대로는 이관 불가다.

### 1.3 이미 엔진에 있는 기능
- `if(...)`, `ifs(...)`, `cast(...)`는 `ExpressionCompiler`의 내장 special function이다.
- `ProductNameContains`, `RiderNameContains`는 이미 `ExpressionFunctions.cs`에 구현되어 있다.

### 1.4 현재 `CommutationTable`에서 바로 쓸 수 있는 데이터
- 문자열: `상품명`, `담보명`
- 스칼라: `n`, `m`, `i`, `v`, `F1` ~ `F10`
- 배열: `q1` ~ `q30`, `k1` ~ `k10`, `r1` ~ `r10`, `Rate_이율`, `Rate_할인율`, `Rate_위험률`
- 누적/통계 배열: `Dx_유지자`, `Dx_납입자`, `Mx_급부` 등

반대로 다음 값들은 현재 `CommutationTable`에 없다.
- `Age`
- `Freq`
- `Amount`
- `Substandard_Mode`
- `S1`, `S5`, `S6`
- scalar `t`

---

## 2. 지금 바로 이관 추천

이 그룹은 현재 구조에서 바로 `ExpressionFunctions.cs`에 추가해도 무리가 없는 함수들이다.

### 2.1 순수 수치 유틸

| helper 원본 | PVPlus2 제안 시그니처 | 우선순위 | 메모 |
|---|---|---|---|
| `RoundUp(double, int)` | `RoundUp(double number, long digits)` | 높음 | 기존 `Round` 계열과 자연스럽게 연결됨 |
| `RoundDown(double, int)` | `RoundDown(double number, long digits)` | 높음 | 기존 `Round` 계열 보강 |
| `Round2(double, int)` | `Round2(double number, long digits)` | 높음 | 레거시의 머신오차 방지 로직 유지 가능 |
| `Round2(double)` | `Round2(double number)` | 높음 | 단축형 |
| `PositiveMin(params double[])` | `PositiveMin(params double[] values)` | 높음 | 순수 함수 |
| `PositiveMax(params double[])` | `PositiveMax(params double[] values)` | 높음 | 순수 함수 |
| `Average(params double[])` | `Average(params double[] values)` | 높음 | 순수 함수 |

### 2.2 선택/검색 유틸

| helper 원본 | PVPlus2 제안 시그니처 | 우선순위 | 메모 |
|---|---|---|---|
| `Choose(int, params int[])` | `Choose(long index, params long[] items)` | 중간 | PVPlus2 정수형 기준으로 long화 |
| `Choose(int, params double[])` | `Choose(long index, params double[] items)` | 높음 | 수식에서 사용성이 높음 |
| `Choose(int, params string[])` | `Choose(long index, params string[] items)` | 높음 | 문자열 분기 대체용 |
| `IndexOf(object, params object[])` | `IndexOf(long item, params long[] items)` | 중간 | object 대신 타입별 오버로드 |
| `IndexOf(object, params object[])` | `IndexOf(double item, params double[] items)` | 중간 | object 대신 타입별 오버로드 |
| `IndexOf(object, params object[])` | `IndexOf(string item, params string[] items)` | 중간 | object 대신 타입별 오버로드 |

### 2.3 문자열 유틸

| helper 원본 | PVPlus2 제안 시그니처 | 우선순위 | 메모 |
|---|---|---|---|
| `Left(string, int)` | `Left(string value, long count)` | 높음 | 직접 이관 가능 |
| `Right(string, int)` | `Right(string value, long count)` | 높음 | 직접 이관 가능 |
| `Mid(string, int, int)` | `Mid(string value, long start, long count)` | 높음 | 1-based 시작 인덱스 유지 |

주의:
- `Left/Right/Mid`의 `int`/`double` 오버로드는 지금 단계에서는 권장하지 않는다.
- 숫자를 문자열처럼 자르는 용도는 `cast(value, string)` 후 문자열 함수로 처리하는 쪽이 더 명확하다.

### 2.4 현재 컨텍스트 기반 조회 유틸

`helper.cs` 원형을 그대로 옮기지는 않지만, 현재 `CommutationTable`만으로 동일 목적을 달성할 수 있는 함수들이다.

| helper 원본 | PVPlus2 제안 시그니처 | 우선순위 | 메모 |
|---|---|---|---|
| `FindQ(string key, int offset)` | `FindQ(CommutationTable context, string key, long offset)` | 높음 | `context.Rate_위험률[key]` 기반으로 구현 가능 |
| `FindQ(int index, int offset)` | `FindQ(CommutationTable context, long index, long offset)` | 높음 | `q1`~`q30` 배열을 직접 선택 가능 |

구현 원칙:
- `offset`은 `checked((int)offset)`로 배열 인덱싱한다.
- 없는 key/index는 명확한 예외 메시지로 실패시킨다.
- `FindQ(long index, long offset)`는 reflection보다 `switch` 또는 명시 분기 형태가 낫다.

---

## 3. 조건부 이관 가능

이 그룹은 함수 자체는 단순하지만, 현재 컨텍스트 모델이나 컴파일러 제약 때문에 바로는 못 옮긴다.

### 3.1 `CommutationTable` 필드 확장 후 가능

| helper 함수 | 필요한 추가 필드 | 비고 |
|---|---|---|
| `Renewal()` | `S1` | 현재 helper는 `S1 > 0` 판정 |
| `AgeSign(int t)` | `Age` | 단순 비교 함수 |
| `S(double K)` | `Substandard_Mode` | `"sub"` 여부 판정 |
| `RoundA(double number)` | `Amount` | 가입금액 기반 반올림 |

권장 시그니처:
- `Renewal(CommutationTable context)`
- `AgeSign(CommutationTable context, long t)`
- `S(CommutationTable context, double k)`
- `RoundA(CommutationTable context, double number)`

### 3.2 컨텍스트 확장만으로는 부족한 함수

| helper 함수 | 추가로 필요한 것 | 막히는 이유 |
|---|---|---|
| `D(params double[] items)` | `t`, `S1` 필드 + compiler 지원 | 현재 엔진은 `context + params` 미지원 |
| `U(params double[] items)` | `t`, `S1`, `Age` 필드 + compiler 지원 | 현재 엔진은 `context + params` 미지원 |

즉, 이 둘은 `CommutationTable`에 필드를 추가하는 것만으로는 부족하고, `ExpressionCompiler`의 함수 인자 변환 규칙도 확장해야 한다.

---

## 4. 이관 비권장 또는 현재 구조상 제외

### 4.1 이미 다른 방식으로 대체된 함수

| helper 함수 | 현재 처리 방식 |
|---|---|
| `Ifs(...)` 전부 | `ExpressionCompiler` 내장 special function `ifs(...)` |
| `ToInt(object)`, `ToDouble(object)`, `ToString(object)` | `cast(...)` 내장 함수로 대체 |

### 4.2 DSL 문자열 파싱 기반 함수

| helper 함수 | 제외 이유 |
|---|---|
| `ToInt(object item, string items)` | `"A->1,B->2"` 같은 ad-hoc mini DSL 파싱이 필요 |
| `ToDouble(object item, string items)` | 수식 엔진 core 함수로 넣기엔 규칙이 특수함 |
| `ToString(object item, string items)` | 동일 |
| `ToIntOrDefault(string, int)` | 문화권/파싱 규칙을 새로 정해야 함 |
| `ToDoubleOrDefault(string, double)` | 문화권/파싱 규칙을 새로 정해야 함 |

이 그룹은 필요성이 확인되면 별도 "문자열 파싱 함수" 묶음으로 설계하는 것이 낫다. 현재 `ExpressionFunctions` 확장 1차 범위에는 넣지 않는다.

### 4.3 디버그/개발 편의 함수

| helper 함수 | 제외 이유 |
|---|---|
| `TypeOf(object)` | 현재 타입 시스템에 `object`가 없음 |
| `ThrowError()` | 디버그용이며 운영 수식 API로 노출할 이유가 약함 |

### 4.4 외부 계산기/전역 상태 의존 함수

아래 함수들은 단순 helper가 아니라 계산 서비스 레이어에 가깝다.

| helper 함수 | 제외 이유 |
|---|---|
| `EVal(...)` | `PVCalculator` 의존 |
| `Ex(...)` | `Expense`/`PVCalculator` 의존 |
| `Pr(...)` 전부 | `PVCalculator`, `LineInfo` 의존 |
| `PrTerm(...)` 전부 | `PVCalculator`, `LineInfo` 의존 |
| `Ax(...)` | 타 담보 계산 + 캐시 + `LineInfo` 의존 |
| `GP(...)` 전부 | `PVCalculator` 의존 |
| `Xx(...)` | 타 담보/타 계산기 의존 |
| `V(...)`, `W(...)` 전부 | `PVCalculator`, `S5/S6`, calculator cache 의존 |
| `GetCacheKey(...)`, `OtherRiderCache` | helper 함수가 아니라 캐시 인프라 |

이 함수들은 `ExpressionFunctions`가 아니라 별도 서비스 계층 또는 계산 컨텍스트 설계가 먼저다.

---

## 5. 권장 이관 순서

### Phase 1
- `RoundUp`
- `RoundDown`
- `Round2`
- `PositiveMin`
- `PositiveMax`
- `Average`
- `Choose`
- `IndexOf`
- `Left`
- `Right`
- `Mid`

이 단계는 전부 pure function이라 리스크가 낮다.

### Phase 2
- `FindQ(CommutationTable context, string key, long offset)`
- `FindQ(CommutationTable context, long index, long offset)`

이 단계는 context-injected 함수지만 현재 `CommutationTable`만으로 닫힌 구현이 가능하다.

### Phase 3
- `Renewal`
- `AgeSign`
- `S`
- `RoundA`

이 단계는 `CommutationTable` 필드 확장이 선행돼야 한다.

### 별도 작업 없이는 보류
- `D`
- `U`

이 둘은 `context + params` 지원을 컴파일러에 추가하기 전에는 넣지 않는다.

---

## 6. 테스트 관점 체크리스트

### 6.1 pure function
- `RoundUp(1.234, 2) == 1.24`
- `RoundDown(1.239, 2) == 1.23`
- `Round2(0.1 + 0.2, 1) == 0.3`
- `PositiveMin(-1, 0, 3, 2) == 2`
- `PositiveMax(-1, 0, 3, 2) == 3`
- `Average(1, 2, 3) == 2`
- `Choose(0, 10, 20, 30) == 10`
- `Choose(9, "a", "b") == "b"`
- `IndexOf("b", "a", "b", "c") == 2`
- `Left("ABCDE", 2) == "AB"`
- `Right("ABCDE", 2) == "DE"`
- `Mid("ABCDE", 2, 3) == "BCD"`

### 6.2 context function
- `FindQ("표준위험률", 3)`가 `Rate_위험률["표준위험률"][3]`과 동일
- `FindQ(1, 5)`가 `q1[5]`와 동일
- 없는 key/index에서 예외가 명확한지 확인

### 6.3 컴파일 실패 케이스
- `Left(123, 2)`는 현재 설계상 컴파일 실패가 맞다.
- `Choose("1", 10, 20)`는 타입 불일치로 컴파일 실패가 맞다.

---

## 7. 최종 권고

현재 기준으로 실제 이관 가치가 높은 함수는 아래 13종이다.

1. `RoundUp`
2. `RoundDown`
3. `Round2`
4. `PositiveMin`
5. `PositiveMax`
6. `Average`
7. `Choose`
8. `IndexOf`
9. `Left`
10. `Right`
11. `Mid`
12. `FindQ(string key, offset)`의 context-injected 변형
13. `FindQ(index, offset)`의 context-injected 변형

반대로 `D`, `U`, `EVal`, `Pr`, `V`, `W`, `Ax`, `GP`, `Xx` 계열은 helper 함수 이관으로 보기보다, 계산 컨텍스트와 서비스 계층 설계 문제로 보는 것이 맞다.
