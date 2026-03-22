# CommutationTable 현황 및 추가 변수 명세

레거시 `helper.cs`의 `variables[]` 참조 분석 및 향후 함수 이관 요건을 기반으로
`CommutationTable.cs`에 추가가 필요한 항목을 정리한다.

레거시 코드 경로: `C:\_z_work\PVPlus2\reference_PVPlus\`

---

## 1. 현재 CommutationTable.cs 현황

### 스칼라 (non-array)

| 이름 | 타입 | 용도 |
|---|---|---|
| `x`, `y` | `double` | TEST 용도 |
| `상품코드`, `판매시기`, `상품명` | `string` | 상품 메타 |
| `담보코드`, `담보명` | `string` | 담보 메타 |
| `예정이율`, `평균공시이율` | `double` | 이율 |
| `판매채널` | **`int`** | 채널 구분 ← 타입 이슈 (하단 참조) |
| `m`, `n` | `long` | 납입기간, 보험기간 |
| `i`, `v` | `double` | 이율, 할인율 |
| `F1`~`F10` | `long` | 범용 플래그 |
| `S1`~`S10` | `long` | 범용 스위치 |

### double[] 배열

| 그룹 | 항목 |
|---|---|
| 이율 | `Rate_이율`, `Rate_할인율`, `Rate_할인율누계`, `Rate_해지율` |
| 잔존율 | `Rate_유지자`, `Rate_납입자`, `Rate_납입자급부`, `Rate_납입면제자급부` |
| 위험률 | `q1`~`q30` |
| 급부계수 | `k1`~`k10` |
| 비율계수 | `r1`~`r10` |
| 생존자수 | `Lx_납입자`, `Lx_유지자`, `Lx_납입면제자` |
| D함수 | `Dx_납입자`, `Dx_유지자` |
| N함수 | `Nx_납입자`, `Nx_유지자` |
| C함수 | `Cx_납입자급부`, `Cx_납입면제자급부` |
| M함수 | `MxSegments_급부합계`, `Mx_납입자급부`, `Mx_납입면제자급부`, `Mx_급부` |

### Dictionary / List 컬렉션

| 이름 | 타입 | 용도 |
|---|---|---|
| `Rate_위험률` | `Dictionary<string, double[]>` | 키-기반 위험률 |
| `RateSegments_급부`, `RateSegments_유지자` | `List<double[]>` | 세그먼트 급부율 |
| `LxSegments_유지자`, `CxSegments_급부`, `MxSegments_급부` | `List<double[]>` | 세그먼트 Lx/Cx/Mx |

---

## 2. 추가가 필요한 스칼라 변수

### 2.1 Age (long) — 피보험자 나이

**레거시 정의 및 흐름**

```
DataReader.cs:256  Context.Variables["Age"] = 0;           // 초기화
DataReader.cs:708  s.Age = ToIntOrDefault(arr[8], 40);     // SInfo.txt 로드 (기본값 40세)
LineInfo.cs:113    variables["Age"] = sInfo.Age;           // 계산 컨텍스트에 할당
```

```csharp
// RULES/SInfo.cs:21 — 데이터 모델
public int Age { get; set; }
```

**사용 예시**

```csharp
// helper.cs:45 — AgeSign 함수
if ((int)variables["Age"] < t) return 0; else return 1;

// PVCALCULATOR/PVCALBase.cs:273
int age = (int)variables["Age"];
```

**값 범위**: 0 이상 정수 (기본값 40)

**PVPlus2 적용**

```csharp
public long Age { get; set; }    // 레거시 int → long
```

---

### 2.2 Freq (long) — 납입 주기

**레거시 정의 및 흐름**

```
DataReader.cs:257  Context.Variables["Freq"] = 0;          // 초기화
DataReader.cs:707  s.Freq = ToIntOrDefault(arr[7], 1);     // SInfo.txt 로드 (기본값 1=연납)
LineInfo.cs:114    variables["Freq"] = sInfo.Freq;         // 계산 컨텍스트에 할당
```

```csharp
// RULES/SInfo.cs:20 — 데이터 모델
public int Freq { get; set; }
```

**사용 예시**

```csharp
// helper.cs:114 — EVal 함수
int freq = (int)variables["Freq"];

// PVCALCULATOR/PVCALBase.cs:57
double payCnt = mm(freq);    // 연간 납입 횟수 계산

// PVResult.cs:533 — 결과 리포트
"납입주기코드(Freq)", variables["Freq"].ToString()
```

**값 범위**:
| 값 | 의미 |
|---|---|
| `1` | 연납 (기본값) |
| `2` | 반년납 |
| `4` | 분기납 |
| `12` | 월납 |
| `99` | 일시납 |

**PVPlus2 적용**

```csharp
public long Freq { get; set; }    // 레거시 int → long
```

---

### 2.3 Substandard_Mode (string) — 표준미달체 모드

**레거시 정의 및 흐름**

```
DataReader.cs:249  Context.Variables["Substandard_Mode"] = "None";   // 초기화
```

```csharp
// RULES/LineInfo.cs:352,355,359,362 — 계산 중 동적 전환
if (standardType > 0 && (string)variables["Substandard_Mode"] == "None")
{
    variables["Substandard_Mode"] = "norm";   // 표준체로 설정 후 계산
    // ... 표준체 계산 ...
    variables["Substandard_Mode"] = "sub";    // 할증체로 전환
    // ... 할증체 계산 ...
    variables["Substandard_Mode"] = "None";   // 원래대로 복구
}
```

**사용 예시**

```csharp
// helper.cs:85 — S 함수 (표준미달 계수)
if ((string)variables["Substandard_Mode"] == "sub") return K;
else return 1.0;
```

**값 범위**:
| 값 | 의미 |
|---|---|
| `"None"` | 기본값, 구분 없음 |
| `"norm"` | 표준체 (1배 위험률) |
| `"sub"` | 할증체 (k배 위험률) |

**비고**: 레거시에서는 계산 루프 내에서 변수를 직접 덮어쓰고 복구하는 방식으로 사용.
PVPlus2에서는 CommutationTable 필드로 노출하되, 설정은 계산 진입 시 외부에서 지정.

**PVPlus2 적용**

```csharp
public string Substandard_Mode { get; set; } = string.Empty;
```

---

### 2.4 Amount (double) — 가입금액

**레거시 정의 및 흐름**

```
DataReader.cs:307  Context.Variables["Amount"] = 0.0;              // 초기화
LineInfo.cs:144    variables["Amount"] = riderRule.가입금액Expr.Evaluate();  // 담보 규칙의 가입금액 수식 평가
```

**사용 예시**

```csharp
// PVCALCULATOR/PVCALBase.cs:48
SA = (double)variables["Amount"];     // SA = Sum Assured (보험가입금액)

// PVCALCULATOR/PVCALSubstandard.cs:34
SA = (double)variables["Amount"];

// helper.cs:433 — RoundA 함수
double Amount = (double)variables["Amount"];
return Round2(number * Amount, 0) / Amount;

// PVResult.cs:587 — 결과 리포트
"가입금액", variables["Amount"].ToString()
```

**값 범위**: 양수 double (담보별 최소/최대 가입금액 규칙 존재)

**PVPlus2 적용**

```csharp
public double Amount { get; set; }
```

---

## 3. 타입 불일치 항목

### 3.1 판매채널 (int → long)

**레거시 정의 및 흐름**

```
DataReader.cs:263  Context.Variables["Channel"] = 0;        // 초기화 (키 이름: "Channel")
DataReader.cs:509  pr.판매채널 = ToIntOrDefault(arr[5], 0); // Product.txt 로드 (기본값 0)
LineInfo.cs:148    variables["Channel"] = productRule.판매채널;  // 할당
```

```csharp
// RULES/ProductRule.cs:16 — 데이터 모델
public int 판매채널 { get; set; }
```

**주목**: 레거시 `variables` 키 이름은 `"Channel"` (영문), 모델 프로퍼티 이름은 `판매채널` (한글).

**문제**: 현재 CommutationTable.cs에서 `int 판매채널`로 선언되어 있으나,
ExpressionCompiler는 정수 리터럴을 `long`으로 파싱하므로
수식 `판매채널 == 1` 비교 시 `int`와 `long` 불일치가 발생한다.

**PVPlus2 적용**

```csharp
// 현재
public int 판매채널 { get; set; }

// 수정 필요
public long 판매채널 { get; set; }
```

**영향 범위**: `CommutationTable.cs` 선언 1행, 초기화/할당 코드 확인 필요

---

## 4. 배열 구조 검토

### 4.1 Dx / Nx 계열 누락 항목

현재 Lx는 납입자/유지자/납입면제자 3종이 있으나, Dx·Nx는 납입자/유지자 2종만 존재.

| 배열명 | 현재 | 검토 |
|---|---|---|
| `Dx_납입면제자` | 없음 | 레거시 `CommutationTable_old.cs`에도 없음 — 의도적 생략 |
| `Nx_납입면제자` | 없음 | 동일 |

→ **현재 설계 유지** 권장

### 4.2 Cx_급부 (합산) 누락

`Mx_급부`는 있으나 원천 `Cx_급부` 합산 배열은 없고, 대신 `MxSegments_급부합계`가 있음.

| 배열명 | 현재 | 검토 |
|---|---|---|
| `Cx_급부` (합산) | 없음 | `MxSegments_급부합계`로 대체 가능한지 계산 로직 확인 필요 |

---

## 5. 추가 순서 권장

| 우선순위 | 항목 | 이유 |
|---|---|---|
| 즉시 | `판매채널` int → long 수정 | 타입 불일치로 수식 오류 발생 가능 |
| 함수 이관 전 | `Age`, `Substandard_Mode`, `Amount` | `AgeSign`, `S`, `RoundA` 함수 이관 시 필요 |
| PVCalculator 연동 전 | `Freq` | 보험료 계산 함수군 이관 시 필요 |
| 별도 검토 | `Dx_납입면제자`, `Nx_납입면제자`, `Cx_급부` | 계산 로직 검토 후 결정 |

---

## 6. 추가 후 CommutationTable 전체 스칼라 목록 (목표 상태)

```csharp
// TEST
public double x { get; set; }
public double y { get; set; }

// 상품/담보 메타
public string 상품코드 { get; set; }
public string 판매시기 { get; set; }
public string 상품명 { get; set; }
public string 담보코드 { get; set; }
public string 담보명 { get; set; }
public double 예정이율 { get; set; }
public double 평균공시이율 { get; set; }
public long 판매채널 { get; set; }              // int → long 수정

// 계약 기본 변수
public long m { get; set; }                     // 납입기간
public long n { get; set; }                     // 보험기간
public long Age { get; set; }                   // ★ 추가: 피보험자 나이 (기본값 40)
public long Freq { get; set; }                  // ★ 추가: 납입 주기 (1/2/4/12/99)

// 이율
public double i { get; set; }
public double v { get; set; }

// 범용 플래그/스위치
public long F1 { get; set; }  // ...  public long F10 { get; set; }
public long S1 { get; set; }  // ...  public long S10 { get; set; }

// 계산 모드
public string Substandard_Mode { get; set; } = string.Empty;   // ★ 추가: "None"/"norm"/"sub"
public double Amount { get; set; }                              // ★ 추가: 가입금액 (SA)
```
