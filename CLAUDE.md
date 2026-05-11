# CLAUDE.md

이 파일은 Claude Code(claude.ai/code)가 이 저장소에서 작업할 때 참고하는 가이드입니다.

## 프로젝트 개요

Unity로 만든 **지하철/도시철도 네트워크 시뮬레이션 게임**. 플레이어가 역을 연결하는 노선을 그리고, 열차를 배치해 승객 흐름을 관리하며 역 과부하를 방지한다. 핵심 실패 조건은 승객이 30초 이상 역 수용 인원을 초과하는 것(오버플로우 타이머).

**Unity 버전**: 6000.3.11f1 (Unity 6)

## 개발 환경 설정

표준 Unity 프로젝트 — Unity Hub에서 Unity 6000.3.11f1로 열기. 별도 빌드 스크립트 없음; Unity Editor 빌드 파이프라인 직접 사용.

**코드 포매팅**: C# 포매팅에 `dotnet-csharpier` 사용.

```powershell
dotnet csharpier .
```

**테스팅**: Unity Test Framework(`com.unity.test-framework` 1.6.0) 포함되어 있으나 아직 작성된 테스트 없음.

## 권한 및 제한 사항

`.claude/settings.json` 기준:
- `.unity`, `.csproj`, `.sln`, `package-lock.json` 파일 **편집 금지**
- `Library/`, `Temp/`, `Logs/` 디렉터리 읽기·수정 **금지**
- 파괴적인 `rm` 명령어 실행 **금지**

## 아키텍처

### 씬 흐름

```
MainTitleScene → MenuScene → Scene (메인 게임플레이)
```

개발자 전용 씬(`Jina.unity`, `Jisu.unity`, `Sky.unity`)은 개인 샌드박스.

### 매니저 계층 구조

모든 매니저는 `Assets/Scripts/`에 위치하며 직접 참조로 통신(DI 컨테이너 없음):

| 매니저 | 역할 |
|---|---|
| `GameManager` | 중앙 게임 상태, 게임 오버 처리, UI 조율 |
| `AssetManager` | 자산 경제: 날짜 카운터, 주간 보상 시스템, 잠금 해제 가능 자산 |
| `MouseInput` | 현재 `Mode` 열거형에 따라 모든 입력을 해당 매니저로 라우팅 |
| `LineManager` | 노선 생성, 편집, 삭제 |
| `TrainManager` | 열차/객차 스폰 및 관리 |
| `StationManager` | 시간 경과에 따른 랜덤 역 스폰 |
| `PassengerManager` | 승객 목적지 배정 |

**씬에서 매니저 찾기**: 매니저는 타입이나 참조가 아닌 태그로 찾음(예: `"GameManager"`, `"PassengerManager"`). 태그는 Inspector에서 설정.

### 입력 상태 머신

`MouseInput.cs`가 중앙 입력 라우터. `Mode` 열거형이 클릭/드래그 시 동작을 제어:

```
None | NewLine | ExtendLine | EditLine | NewTrain | HighTrain | InterchangeStation | Carriage
```

모드 전환은 UI 버튼에 의해 트리거되고 `MouseInput`을 통해 전달됨.

### 역 오버플로우 및 게임 오버

- 각 역은 30초 오버플로우 타이머를 추적(`StationTimerUI.cs`)
- `Station.OnTimeOver`는 **정적 이벤트** — `GameManager`가 구독
- 발동 시 GameManager가 게임 오버 시퀀스 트리거

### 열차 길 찾기

`Train.cs`(~731줄)에 모든 길 찾기 및 이동 로직 포함:

- **`BFSDistance()`** — 연결된 모든 노선에서 목적지 역까지 최단 거리 탐색
- **`BFS()`** — 도달 가능성 확인(환승 계획에 사용)
- **`CanBoard()`** — 대기 중인 승객이 도달 가능성 기준으로 이 열차에 탑승 가능한지 검증
- **`FindTransferStation()`** — 다중 노선 여정의 최적 환승 지점 결정

순환형 vs. 직선형 노선 토폴로지에 따라 열차 동작 변경: 직선 노선은 종점에서 방향 전환; 순환 노선은 계속 순환.

### 객차 추종 시스템

열차가 이동하면서 위치 기록을 저장. 객차는 이 기록을 따라 보간하여 부드러운 "뱀" 추종 효과 생성. `Train.cs`가 위치 기록 목록을 유지하는 이유.

### 주간 보상 시스템

`AssetManager`가 경과 일수를 추적. 매 7일마다 게임플레이가 멈추고 랜덤 자산 선택 UI 표시(`GameManager`에서 처리). 잠금 해제 가능 자산: 환승역, 추가 열차, 객차, 고속 열차.

### 전역 정적 상태

- `Score` — 정적 클래스, `Score.score`(int)로 현재 점수 추적
- `Colors` — 정적 클래스, 최대 7개 노선을 위한 7색 배열
- `AssetManager.dayCount` — 정적 날짜 카운터

### 주요 데이터 타입

- `StationType` 열거형: `Circle`, `Square`, `Triangle` — 어떤 승객이 하차 가능한지 결정
- `PassengerState` 열거형: `Waiting` → `OnTrain` → `Arrived`
- `TrainDirection` 열거형: `Forward` / `Backward`

## 코드 컨벤션

- **언어**: Unity MonoBehaviour 패턴을 사용하는 C#
- **주석과 디버그 로그는 한국어로 작성** (역, 열차, 승객, 환승 등)
- 클래스와 public 멤버는 PascalCase; private 멤버는 camelCase
- private 필드 접두사 불일치(`_variable`과 일반 `camelCase` 혼용)
- 객체 간 식별에 태그 사용 — 새 태그 추가 전 `CompareTag()` 호출 확인
- `Assets/Resources/`에서 런타임 스프라이트 로딩에 `Resources.Load()` 사용

## 주요 패키지

- `com.unity.render-pipelines.universal` 17.3.0 — URP 렌더링 파이프라인
- `com.unity.inputsystem` 1.19.0 — New Input System(`MouseInput.cs`에서 사용)
- `com.unity.2d.sprite` — 2D 스프라이트 지원
- `com.unity.timeline` 1.8.11 — 컷씬/크레딧 애니메이션에 사용
