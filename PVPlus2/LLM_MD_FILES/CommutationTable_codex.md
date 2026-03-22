# CommutationTable_codex

`PVPlus2/Models/CommutationTable.cs`를 기준으로, 레거시 PVPlus 변수 체계와 비교했을 때 추가로 필요할 수 있는 변수들을 정리한 문서다.

이 문서는 단순히 "레거시에 있었던 변수"를 나열하지 않고, 아래 3가지로 나눈다.

- `CommutationTable`에 실제로 새 필드를 추가하는 것이 좋은 경우
- 기존 필드의 alias 또는 의미 매핑으로 해결하는 경우
- `CommutationTable` 필드가 아니라 compiler/service 레이어에서 해결하는 것이 맞는 경우

---

## 1. 현재 상태 요약

현재 `CommutationTable`에는 이미 다음 축이 들어 있다.

- 상품/담보 메타: `상품코드`, `판매시기`, `상품명`, `판매채널`, `담보코드`, `담보명`
- 기본 수치: `n`, `m`, `i`, `ii`, `v`, `vv`
- factor: `F1` ~ `F10`
- 상태값: `S1` ~ `S10`
- 배열/율: `Rate_*`, `q1` ~ `q30`, `k1` ~ `k10`, `r1` ~ `r10`
- 기수표/누계: `Lx_*`, `Dx_*`, `Nx_*`, `Cx_*`, `Mx_*`, `RateSegments_*`

즉, 예전 기준으로 빠져 있다고 보았던 것 중 아래는 이미 충족되었다.

- `S1`, `S5`, `S6` 포함 `S1` ~ `S10`
- `ii`
- `vv`

---

## 2. 실제 추가가 필요한 변수

이 그룹은 레거시 helper 함수, 조건식, 산출식 이관을 생각하면 `CommutationTable`에 직접 들어가는 편이 맞다.

### 2.1 1순위

| 변수명 | 권장 타입 | 필요 이유 |
|---|---|---|
| `Age` | `long` | `AgeSign`, `U`, 연장형 계산, `F6 - Age` 계열 식 지원 |
| `Freq` | `long` | `Eval(..., Freq)`, `RoundA`, `V/W`, 각종 보험료/준비금 식 지원 |
| `Amount` | `double` | `RoundA`, `NP0`, `GP0`, `V0`, `W0` 등 금액 환산식 지원 |
| `Substandard_Mode` | `string` | `S(double K)` 및 표준형/저해지 분기 지원 |

권장 초기값:

```csharp
public long Age { get; set; }
public long Freq { get; set; }
public double Amount { get; set; }
public string Substandard_Mode { get; set; } = "None";
```

### 2.2 2순위

| 변수명 | 권장 타입 | 필요 이유 |
|---|---|---|
| `PV_Type` | `long` | 레거시 조건식과 calculator 분기 기준 |
| `S_Type` | `long` | `STDALPHA_UNIT` 계열 분기 조건 |
| `Jong` | `long` | SInfo/요약/조건 분기 호환 |

권장 형태:

```csharp
public long PV_Type { get; set; }
public long S_Type { get; set; }
public long Jong { get; set; }
```

### 2.3 3순위

| 변수명 | 권장 타입 | 필요 이유 |
|---|---|---|
| `ElapseYear` | `long` | 결과 요약식 `V0`, `V1`, `W0`, `W1`, `TempCK0`, `TempCK1` 호환 |

설명:
- 레거시에서 `ElapseYear`는 결과 조회/요약 시점 변수로 많이 쓰였다.
- 내부 array loop index인 `t`와 비슷해 보이지만, 의미가 항상 완전히 같은 것은 아니다.
- 결과 집계/출력용 수식까지 포팅할 생각이면 별도 필드가 있는 편이 안전하다.

권장 형태:

```csharp
public long ElapseYear { get; set; }
```

---

## 2.4 reference_PVPlus 근거

아래는 위에서 추천한 변수들이 레거시 `reference_PVPlus`에서 실제로 어떻게 정의되고 사용되었는지 정리한 것이다.

### `Age`

정의/초기화:
- [`RULES/DataReader.cs:256`](/C:/_z_work/PVPlus2/reference_PVPlus/RULES/DataReader.cs#L256)에서 `Context.Variables["Age"] = 0;`
- [`RULES/SInfo.cs:21`](/C:/_z_work/PVPlus2/reference_PVPlus/RULES/SInfo.cs#L21)에서 `Age` 필드 보유
- [`RULES/LineInfo.cs:113`](/C:/_z_work/PVPlus2/reference_PVPlus/RULES/LineInfo.cs#L113)에서 `variables["Age"] = sInfo.Age;`

사용:
- [`helper.cs:43`](/C:/_z_work/PVPlus2/reference_PVPlus/helper.cs#L43) `AgeSign(int t)`에서 `(int)variables["Age"] < t`
- [`helper.cs:73`](/C:/_z_work/PVPlus2/reference_PVPlus/helper.cs#L73) `U(params double[])`에서 `AgeSign(15)`
- [`ChkExprs.txt:69`](/C:/_z_work/PVPlus2/reference_PVPlus/ChkExprs.txt#L69) `Min(F6-Age,20)` 패턴 사용
- [`ChkExprs.txt:70`](/C:/_z_work/PVPlus2/reference_PVPlus/ChkExprs.txt#L70) 동일

해석:
- `Age`는 helper 함수뿐 아니라 실제 상품 수식에서도 직접 참조된다.
- 따라서 단순 부가 메타가 아니라 계산 핵심 입력이다.

### `Freq`

정의/초기화:
- [`RULES/DataReader.cs:257`](/C:/_z_work/PVPlus2/reference_PVPlus/RULES/DataReader.cs#L257)에서 `Context.Variables["Freq"] = 0;`
- [`RULES/SInfo.cs:20`](/C:/_z_work/PVPlus2/reference_PVPlus/RULES/SInfo.cs#L20)에서 `Freq` 필드 보유
- [`RULES/LineInfo.cs:114`](/C:/_z_work/PVPlus2/reference_PVPlus/RULES/LineInfo.cs#L114)에서 `variables["Freq"] = sInfo.Freq;`

사용:
- [`helper.cs:114`](/C:/_z_work/PVPlus2/reference_PVPlus/helper.cs#L114) `EVal(string chkItem)`에서 `freq = (int)variables["Freq"]`
- [`helper.cs:139`](/C:/_z_work/PVPlus2/reference_PVPlus/helper.cs#L139) `Pr(int age, int n, int m, int freq)`용 otherVariables에 `Freq`
- [`ChkExprs.txt:17`](/C:/_z_work/PVPlus2/reference_PVPlus/ChkExprs.txt#L17) `Eval(..., Freq)`
- [`ChkExprs.txt:19`](/C:/_z_work/PVPlus2/reference_PVPlus/ChkExprs.txt#L19) `RoundA(Eval("V_UNIT",n,m,t,Freq))`
- [`ChkExprs.txt:61`](/C:/_z_work/PVPlus2/reference_PVPlus/ChkExprs.txt#L61) `S4>0 OR Freq=99`
- [`ChkExprs.txt:63`](/C:/_z_work/PVPlus2/reference_PVPlus/ChkExprs.txt#L63) `(12.0/Freq)*...`

해석:
- `Freq`는 helper 호출 인자, 조건식, 보험료/준비금 계산 모두에서 중심 변수다.
- `CommutationTable` 필드로 넣는 편이 맞다.

### `Amount`

정의/초기화:
- [`RULES/DataReader.cs:307`](/C:/_z_work/PVPlus2/reference_PVPlus/RULES/DataReader.cs#L307)에서 `Context.Variables["Amount"] = 0.0;`
- [`RULES/LineInfo.cs:144`](/C:/_z_work/PVPlus2/reference_PVPlus/RULES/LineInfo.cs#L144)에서 `variables["Amount"] = riderRule.가입금액Expr.Evaluate();`
- [`RULES/RiderRule.cs:17`](/C:/_z_work/PVPlus2/reference_PVPlus/RULES/RiderRule.cs#L17)에서 `가입금액Expr`

사용:
- [`helper.cs:431`](/C:/_z_work/PVPlus2/reference_PVPlus/helper.cs#L431) `RoundA(double number)`에서 직접 사용
- [`ChkExprs.txt:20`](/C:/_z_work/PVPlus2/reference_PVPlus/ChkExprs.txt#L20) `Round2(Eval("NP_UNIT",n,m,0,Freq)*Amount,0)`
- [`ChkExprs.txt:42`](/C:/_z_work/PVPlus2/reference_PVPlus/ChkExprs.txt#L42) `Eval("V_UNIT",n,m,ElapseYear,Freq)*Amount`
- [`ChkExprs.txt:69`](/C:/_z_work/PVPlus2/reference_PVPlus/ChkExprs.txt#L69) `...*Amount,0)/Amount`

해석:
- `Amount`는 금액 환산의 기준값이라 helper/수식 양쪽에서 매우 빈번하게 쓰인다.

### `Substandard_Mode`

정의/초기화:
- [`RULES/DataReader.cs:249`](/C:/_z_work/PVPlus2/reference_PVPlus/RULES/DataReader.cs#L249)에서 `Context.Variables["Substandard_Mode"] = "None";`
- [`RULES/LineInfo.cs:352`](/C:/_z_work/PVPlus2/reference_PVPlus/RULES/LineInfo.cs#L352) 이후 표준형 계산 분기에서 `"None"`, `"norm"`, `"sub"` 값을 순환 사용

사용:
- [`helper.cs:83`](/C:/_z_work/PVPlus2/reference_PVPlus/helper.cs#L83) `S(double K)`에서 `"sub"`일 때만 `K` 반환
- [`RULES/LineInfo.cs:352`](/C:/_z_work/PVPlus2/reference_PVPlus/RULES/LineInfo.cs#L352) 표준형 계산 분기 진입 조건
- [`RULES/LineInfo.cs:355`](/C:/_z_work/PVPlus2/reference_PVPlus/RULES/LineInfo.cs#L355) `"norm"`
- [`RULES/LineInfo.cs:359`](/C:/_z_work/PVPlus2/reference_PVPlus/RULES/LineInfo.cs#L359) `"sub"`
- [`RULES/LineInfo.cs:362`](/C:/_z_work/PVPlus2/reference_PVPlus/RULES/LineInfo.cs#L362) `"None"` 복원

해석:
- 이 값은 단순 표시용이 아니라 표준형/저해지 분기를 제어하는 상태값이다.

### `PV_Type`

정의/초기화:
- [`RULES/DataReader.cs:264`](/C:/_z_work/PVPlus2/reference_PVPlus/RULES/DataReader.cs#L264)에서 `Context.Variables["PV_Type"] = 0;`
- [`RULES/RiderRule.cs:16`](/C:/_z_work/PVPlus2/reference_PVPlus/RULES/RiderRule.cs#L16)에서 `PV_Type`는 `IGenericExpression<int>`
- [`RULES/LineInfo.cs:145`](/C:/_z_work/PVPlus2/reference_PVPlus/RULES/LineInfo.cs#L145)에서 `variables["PV_Type"] = riderRule.PV_Type.Evaluate();`

사용:
- [`helper.cs:147`](/C:/_z_work/PVPlus2/reference_PVPlus/helper.cs#L147) `PrTerm(int freq)`에서 otherVariables에 `PV_Type = 1`
- [`helper.cs:159`](/C:/_z_work/PVPlus2/reference_PVPlus/helper.cs#L159) 다른 overload에서도 `PV_Type = 1`
- [`RULES/LineInfo.cs:336`](/C:/_z_work/PVPlus2/reference_PVPlus/RULES/LineInfo.cs#L336) calculator 생성 기준값
- [`ChkExprs.txt:59`](/C:/_z_work/PVPlus2/reference_PVPlus/ChkExprs.txt#L59) `S_Type=1, PV_Type=92`
- [`ChkExprs.txt:69`](/C:/_z_work/PVPlus2/reference_PVPlus/ChkExprs.txt#L69) `S_Type=1, PV_Type=96`

해석:
- `PV_Type`는 레거시에서 수식 조건과 calculator 선택 둘 다에 사용됐다.

### `S_Type`

정의/초기화:
- [`RULES/DataReader.cs:265`](/C:/_z_work/PVPlus2/reference_PVPlus/RULES/DataReader.cs#L265)에서 `Context.Variables["S_Type"] = 0;`
- [`RULES/RiderRule.cs:53`](/C:/_z_work/PVPlus2/reference_PVPlus/RULES/RiderRule.cs#L53)에서 `S_Type`는 `IGenericExpression<int>`
- [`RULES/LineInfo.cs:146`](/C:/_z_work/PVPlus2/reference_PVPlus/RULES/LineInfo.cs#L146)에서 `variables["S_Type"] = riderRule.S_Type.Evaluate();`

사용:
- [`ChkExprs.txt:13`](/C:/_z_work/PVPlus2/reference_PVPlus/ChkExprs.txt#L13) `S_Type=0`
- [`ChkExprs.txt:14`](/C:/_z_work/PVPlus2/reference_PVPlus/ChkExprs.txt#L14) `S_Type=1`
- [`ChkExprs.txt:15`](/C:/_z_work/PVPlus2/reference_PVPlus/ChkExprs.txt#L15) `S_Type=2`
- [`ChkExprs.txt:16`](/C:/_z_work/PVPlus2/reference_PVPlus/ChkExprs.txt#L16) `S_Type=3`

해석:
- `S_Type`는 표준형 수수료/보정식 분기의 핵심 조건 변수다.

### `Jong`

정의/초기화:
- [`RULES/DataReader.cs:258`](/C:/_z_work/PVPlus2/reference_PVPlus/RULES/DataReader.cs#L258)에서 `Context.Variables["Jong"] = 0;`
- [`RULES/SInfo.cs:19`](/C:/_z_work/PVPlus2/reference_PVPlus/RULES/SInfo.cs#L19)에서 `Jong` 필드 보유
- [`RULES/LineInfo.cs:112`](/C:/_z_work/PVPlus2/reference_PVPlus/RULES/LineInfo.cs#L112)에서 `variables["Jong"] = sInfo.Jong;`

사용:
- [`RULES/LineInfo.cs:275`](/C:/_z_work/PVPlus2/reference_PVPlus/RULES/LineInfo.cs#L275) line summary 생성에 반영
- [`RULES/LineInfo.cs:296`](/C:/_z_work/PVPlus2/reference_PVPlus/RULES/LineInfo.cs#L296) key 생성에 포함
- [`RULES/LineInfo.cs:312`](/C:/_z_work/PVPlus2/reference_PVPlus/RULES/LineInfo.cs#L312) 다른 key 생성에도 포함

해석:
- 현재 확인된 흔적은 계산 수식 직접 참조보다 식별/그룹핑 쪽이 강하다.
- 그래도 레거시 호환 목적이라면 필드로 두는 편이 안정적이다.

### `ElapseYear`

정의/초기화:
- [`RULES/DataReader.cs:259`](/C:/_z_work/PVPlus2/reference_PVPlus/RULES/DataReader.cs#L259)에서 `Context.Variables["ElapseYear"] = 0;`

사용:
- [`ChkExprs.txt:42`](/C:/_z_work/PVPlus2/reference_PVPlus/ChkExprs.txt#L42) `V0 = Eval("V_UNIT",n,m,ElapseYear,Freq)`
- [`ChkExprs.txt:43`](/C:/_z_work/PVPlus2/reference_PVPlus/ChkExprs.txt#L43) `V1 = Eval("V_UNIT",n,m,ElapseYear+1,Freq)`
- [`ChkExprs.txt:45`](/C:/_z_work/PVPlus2/reference_PVPlus/ChkExprs.txt#L45) `W0 = Eval("W_UNIT",n,m,ElapseYear,Freq)`
- [`ChkExprs.txt:46`](/C:/_z_work/PVPlus2/reference_PVPlus/ChkExprs.txt#L46) `W1 = Eval("W_UNIT",n,m,ElapseYear+1,Freq)`
- [`ChkExprs.txt:101`](/C:/_z_work/PVPlus2/reference_PVPlus/ChkExprs.txt#L101) `If(ElapseYear<m, 0, ...)`
- [`PVResult.cs:88`](/C:/_z_work/PVPlus2/reference_PVPlus/PVResult.cs#L88) 등 결과 계산/출력에서 반복 사용

해석:
- `ElapseYear`는 내부 loop용 `t`와 별개로 "결과 조회 시점" 의미가 강하다.

### `Channel`

정의/초기화:
- [`RULES/DataReader.cs:263`](/C:/_z_work/PVPlus2/reference_PVPlus/RULES/DataReader.cs#L263)에서 `Context.Variables["Channel"] = 0;`
- [`RULES/LineInfo.cs:148`](/C:/_z_work/PVPlus2/reference_PVPlus/RULES/LineInfo.cs#L148)에서 `variables["Channel"] = productRule.판매채널;`

사용:
- [`ChkExprs.txt:14`](/C:/_z_work/PVPlus2/reference_PVPlus/ChkExprs.txt#L14) `If(Channel>0, 0.7, 1)`
- [`ChkExprs.txt:59`](/C:/_z_work/PVPlus2/reference_PVPlus/ChkExprs.txt#L59) 동일 패턴
- [`ChkExprs.txt:74`](/C:/_z_work/PVPlus2/reference_PVPlus/ChkExprs.txt#L74) 동일 패턴

해석:
- 레거시 식에서는 `판매채널`이 아니라 `Channel` 이름으로 직접 참조됐다.
- 필드 추가보다 alias가 적절하다.

### `RiderCode`

정의/초기화:
- [`RULES/DataReader.cs:261`](/C:/_z_work/PVPlus2/reference_PVPlus/RULES/DataReader.cs#L261)에서 `Context.Variables["RiderCode"] = "";`
- [`RULES/LineInfo.cs:24`](/C:/_z_work/PVPlus2/reference_PVPlus/RULES/LineInfo.cs#L24)와 [`RULES/LineInfo.cs:38`](/C:/_z_work/PVPlus2/reference_PVPlus/RULES/LineInfo.cs#L38)에서 `RiderCode` 확보

사용:
- [`helper.cs:100`](/C:/_z_work/PVPlus2/reference_PVPlus/helper.cs#L100) `FindRiderRule(lineInfo.RiderCode)`
- [`helper.cs:178`](/C:/_z_work/PVPlus2/reference_PVPlus/helper.cs#L178) rate key 조회에 사용
- [`RULES/LineInfo.cs:564`](/C:/_z_work/PVPlus2/reference_PVPlus/RULES/LineInfo.cs#L564) 요약 key 생성에 사용

해석:
- 현재 `CommutationTable`의 `담보코드`와 사실상 같은 축이다.
- 별도 신설보다 alias가 낫다.

### `t`

정의/초기화:
- [`RULES/DataReader.cs:306`](/C:/_z_work/PVPlus2/reference_PVPlus/RULES/DataReader.cs#L306)에서 `Context.Variables["t"] = 0;`

사용:
- [`helper.cs:57`](/C:/_z_work/PVPlus2/reference_PVPlus/helper.cs#L57) `D(params double[])`
- [`helper.cs:71`](/C:/_z_work/PVPlus2/reference_PVPlus/helper.cs#L71) `U(params double[])`
- [`helper.cs:113`](/C:/_z_work/PVPlus2/reference_PVPlus/helper.cs#L113) `EVal`
- [`ChkExprs.txt:17`](/C:/_z_work/PVPlus2/reference_PVPlus/ChkExprs.txt#L17) `t<Min(7,m)`
- [`ChkExprs.txt:44`](/C:/_z_work/PVPlus2/reference_PVPlus/ChkExprs.txt#L44) `VWhole`

해석:
- 레거시에선 명시적인 scalar variable이었다.
- PVPlus2에서는 array loop index로 이미 부분 대체하고 있지만, 모든 레거시 식을 그대로 살릴 거면 추가 설계가 더 필요하다.

---

## 3. 새 필드보다 alias가 나은 변수

이 그룹은 굳이 중복 필드를 늘리기보다, 기존 필드 이름과 레거시 이름을 연결하는 방식이 낫다.

| 레거시 이름 | 현재 대응 후보 | 권장 방식 |
|---|---|---|
| `Channel` | `판매채널` | alias property 또는 identifier alias |
| `RiderCode` | `담보코드` | alias property 또는 identifier alias |

권장안:

```csharp
public long Channel
{
    get => 판매채널;
    set => 판매채널 = checked((int)value);
}

public string RiderCode
{
    get => 담보코드;
    set => 담보코드 = value;
}
```

주의:
- `판매채널`은 현재 `int`이므로 `Channel` alias를 둘 경우 `long`과의 변환 규칙을 명시해야 한다.
- alias를 모델에 둘지, compiler의 identifier alias로 해결할지는 별도 선택 사항이다.

---

## 4. `CommutationTable` 필드가 아닌 쪽이 맞는 변수

### 4.1 `t`

`t`는 중요하지만, 무조건 `CommutationTable`에 저장해야 하는 변수는 아니다.

이유:
- 현재 array expression에서는 compiler가 내부 loop index를 `t`로 이미 제공한다.
- `D`, `U`, array 식, `if(t < 2, ...)` 같은 케이스는 이 방식이 더 자연스럽다.

권장:
- array compile 경로에서는 지금처럼 compiler-managed virtual variable로 유지
- scalar helper 호출이나 결과 수식까지 같은 이름으로 통일하고 싶다면 그때 `public long t { get; set; }`를 검토

정리:
- `t`는 "필요한 개념"은 맞다.
- 하지만 1차적으로는 `CommutationTable` 추가 필드보다 compiler special handling 쪽이 우선이다.

### 4.2 `w`

레거시의 `w`는 현재 시점 해지율 scalar로 쓰였지만, 현재 모델에는 이미 `Rate_해지율[]` 배열이 있다.

권장:
- `w`를 별도 필드로 두지 말고, 필요 시 `Rate_해지율[t]`에서 읽도록 한다.
- 정말 legacy expression을 문자열 그대로 살려야 하면, compiler special alias 또는 helper 함수로 푸는 편이 낫다.

### 4.3 `TempStr1`, `TempStr2`

이 둘은 계산 핵심값이라기보다 임시 문자열/디버그 출력에 가깝다.

권장:
- 1차 포팅 범위에서는 추가하지 않는다.
- UI 또는 보고서 출력에서 실제 요구가 생길 때 별도 필드로 추가한다.

### 4.4 `Company`

현재 reference에서는 주로 로딩/환경 정보 쪽에서 초기화되며, 핵심 수식 변수로 쓰이는 흔적은 약하다.

권장:
- `CommutationTable` 핵심 필드로는 보류
- 필요 시 `string Company` 또는 한국어 필드명 `회사`를 별도 메타 영역으로 추가

---

## 5. 권장 추가 세트

실제 우선순위를 기준으로 하면 아래 순서가 적절하다.

### Phase 1

```csharp
public long Age { get; set; }
public long Freq { get; set; }
public double Amount { get; set; }
public string Substandard_Mode { get; set; } = "None";
```

이 단계만으로 바로 좋아지는 것:
- `Renewal`, `AgeSign`, `S`, `RoundA`, `U` 후보 정리
- `Freq`, `Amount`를 쓰는 legacy 수식 호환성 상승
- 표준형/저해지 분기 지원 기반 확보

### Phase 2

```csharp
public long PV_Type { get; set; }
public long S_Type { get; set; }
public long Jong { get; set; }
public long ElapseYear { get; set; }
```

이 단계가 필요한 이유:
- `ChkExprs` 조건식
- 결과 요약/출력식
- calculator 선택 기준

### Phase 3

alias 또는 별도 설계:

```csharp
public long Channel { get; set; } // 또는 판매채널 alias
public string RiderCode { get; set; } = string.Empty; // 또는 담보코드 alias
```

이 단계는 실제 문자열 호환성을 얼마나 강하게 요구하느냐에 따라 선택한다.

---

## 6. `D/U` 관점에서 보면 무엇이 필요한가

### `D(params double[] items)`

레거시 의미:

```csharp
if (Renewal() || t >= items.Length) return 1.0;
return items[t];
```

현재 상태:
- `S1`은 이미 `CommutationTable`에 있음
- 부족한 것은 `t` 접근 방식

정리:
- `D` 지원을 위해 새로 꼭 필요한 `CommutationTable` 필드는 없다
- 대신 `t`를 compiler가 special variable로 계속 제공하거나, 필요 시 `long t`를 추가하면 된다

### `U(params double[] items)`

레거시 의미:

```csharp
if (Renewal() || AgeSign(15) == 0 || t >= items.Length) return 1.0;
return items[t];
```

현재 상태:
- `S1`은 이미 있음
- 추가로 `Age`가 필요
- `t` 접근 방식도 필요

정리:
- `U` 지원의 핵심 추가 필드는 `Age`

---

## 7. 최종 권고

지금 `CommutationTable`에 추가할 값만 좁게 보면 우선순위는 아래와 같다.

1. `Age`
2. `Freq`
3. `Amount`
4. `Substandard_Mode`
5. `PV_Type`
6. `S_Type`
7. `Jong`
8. `ElapseYear`

그리고 별도 판단 항목은 아래다.

- `Channel`: 새 필드보다 `판매채널` alias 권장
- `RiderCode`: 새 필드보다 `담보코드` alias 권장
- `t`: `CommutationTable`보다 compiler-managed variable 우선
- `w`: `Rate_해지율[t]`로 해소 권장
- `TempStr1`, `TempStr2`, `Company`: 1차 포팅 범위에서는 보류

즉, 현재 `CommutationTable`은 기수표/율/상태값 쪽은 많이 갖췄고, 앞으로 진짜 부족한 것은 "나이/납입주기/가입금액/표준형 모드/레거시 조건식 메타" 축이라고 보면 된다.
