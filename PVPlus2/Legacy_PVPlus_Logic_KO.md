# 과거 PVPlus 로직 정리

## 개요

과거 `reference_PVPlus` 프로젝트는 Windows Forms 기반 프로그램이다. 이 프로그램은 다음 요소를 조합해서 P/V/S 테이블을 검증하거나 계산한다.

- 사용자가 선택한 Excel 파일 경로
- Excel 파일과 같은 위치에 있는 `Data` 폴더
- 사용자가 선택한 `P`, `V`, `S` 테이블 파일
- 회사별 라인 보정 규칙
- 수식 컴파일 기반 규칙 엔진

구조적으로는 `Configure`, `PV`, `DataReader`를 통한 전역 static 상태 의존도가 매우 높다.

## 메인 실행 흐름

1. `UI/MainPVForm.cs`에서 사용자 입력을 받는다.
2. `SetConfigure()`에서 Excel 파일 옆의 `Data` 폴더를 찾고, 설정값을 `Configure`에 저장한다.
3. `PV.Run()` 또는 다른 진입 함수에서 `SetData()`를 호출한다.
4. `SetData()`는 `DataReader`를 생성하거나 갱신하고, 이어서 `RuleFinder`를 만들고 helper 상태와 요약 컬렉션을 초기화한다.
5. 선택된 P/V/S 테이블 파일을 한 줄씩 읽는다.
6. 각 줄을 `LineInfo`로 변환한다.
7. `LineInfo`는 layout, 변수, 위험률, 사업비, S 관련 데이터를 해석한다.
8. 계산기(`PVCalculator` 계열)가 `PVResult`를 만든다.
9. 결과 파일을 원본 테이블 파일 옆에 출력한다.

## 설정 상태

`RULES/Configure.cs`는 프로세스 전체에서 공유되는 상태를 담고 있다.

- `WorkingDI`: `Data` 폴더
- `PVSTableInfo`: 선택한 테이블 파일
- `TableType`: `P`, `V`, `SRatio`, `StdAlpha`
- `CompanyRule`
- `ProductCode`
- 구분자 모드와 delimiter
- 한도 체크 여부
- 라인 요약 여부

이 방식은 연결은 쉽지만, 모든 단계가 전역 가변 상태에 강하게 묶인다.

## Data 폴더에서 읽는 txt 파일

`RULES/DataReader.cs`는 `Configure.WorkingDI` 아래의 탭 구분 txt 파일들을 읽는다.

- `Product.txt`
  - 선택한 상품코드에 해당하는 `ProductRule` 1건을 읽는다.
- `Rider.txt`
  - 선택한 상품코드에 해당하는 `RiderRule`들을 읽는다.
  - 추가로 `정기사망` 기본 담보를 코드에서 강제로 넣는다.
- `Expense.txt`
  - 사업비 규칙을 읽는다.
  - 한 행의 담보코드가 콤마로 나뉘어 있으면 여러 규칙으로 확장한다.
- `Rate.txt`
  - 위험률 이름과 연령별 배열값을 포함한 전체 위험률 행을 읽는다.
- `LayoutP.txt`, `LayoutV.txt`, `LayoutS.txt`
  - 현재 테이블 종류에 맞는 layout 정의를 읽는다.
- `VarChg.txt`
  - `Base`, 상품 단위, 상품+담보 단위의 변수 변경식을 읽는다.
- `EvaluatedSInfo.txt`
  - 파일이 있으면 평가 완료된 S 정보를 읽는다.
- `ChkExprs.txt`
  - 회사별 검증 수식을 읽는다.
- `Sinfo.txt`
  - S 계산 경로에서만 읽고, 이후 `EvaluatedSInfo.txt`로 다시 쓴다.

## 파싱 및 수식 처리

과거 로더는 비교적 단순한 방식으로 txt를 읽는다.

- `ToArrList(path)`가 파일 전체를 메모리로 읽는다.
- 인코딩은 `Encoding.Default`를 사용한다.
- 각 줄을 `Split('\t')`로 나눈다.

많은 컬럼은 단순 값이 아니라 Flee 식으로 컴파일된다.

- 정수 수식
- 실수 수식
- 문자열 수식
- 불리언 수식
- 동적 수식

컴파일 결과는 `DataReader` 내부에 캐시된다.

## 조회 계층

`RULES/RuleFinder.cs`는 읽어온 리스트를 조회용 구조로 재구성한다.

- 담보코드 기준 rider 조회
- `상품|담보` 기준 변수 변경 조회
- `상품|담보` 기준 사업비 조회
- 위험률명 기준 위험률 조회
- `MinSKey` 기준 S 정보 조회
- 산출항목 기준 검증식 조회

즉 과거 프로젝트에서 `RuleFinder`는 로드된 데이터와 실제 계산 사이의 중간 조회 계층 역할을 한다.

## 라인 단위 처리

`RULES/LineInfo.cs`는 원본 테이블 한 줄을 계산 가능한 변수 집합으로 바꾸는 핵심 클래스다.

한 줄 처리 시 다음 순서로 동작한다.

1. 회사 규칙으로 원본 라인을 보정한다.
2. 구분자 방식 또는 고정폭 방식으로 파싱한다.
3. 담보코드를 판별한다.
4. 적용할 layout을 선택한다.
5. layout 값을 공용 변수 컬렉션에 넣는다.
6. `VarChg` 규칙으로 변수값을 덮어쓴다.
7. 상품, 담보, 위험률, 사업비, S 정보를 조회한다.
8. 계산기 입력을 구성한다.

## 출력 동작

메인 검증 실행에서는 옵션에 따라 다음 파일들을 출력한다.

- 정상건
- 오차건
- 오차건 원본
- 오류건 원본
- 한도초과건
- 라인 요약 파일

S 계산 경로에서는 `EvaluatedSInfo.txt`도 생성한다.

## 과거 구조의 특징

- 전역 static 상태 의존도가 높다.
- 대부분의 참조 데이터를 먼저 `List`로 읽어 둔다.
- 로드 후에 별도 조회 구조를 만든다.
- 수식 컴파일이 로딩 단계와 섞여 있다.
- txt 파싱이 탭 구분 중심으로 비교적 관대하다.
- 인코딩이 Windows 기본 인코딩에 의존한다.

## PVPlus2 재구현 관점에서의 시사점

과거 프로젝트는 비즈니스 규칙과 파일 의미를 파악하는 참고 자료로는 좋지만, 구조 자체를 그대로 복사하는 대상은 아니다.

PVPlus2에서는 다음 방향이 더 적절하다.

- 전역 static 대신 인스턴스 기반 데이터 소유
- 원본 데이터 로딩과 수식 컴파일 분리
- `List` 중심 탐색 대신 `Dictionary` 기반 인덱싱
- 인코딩/검증/파싱 규칙의 명시화
- UI 계층과 계산/로드 계층 분리
