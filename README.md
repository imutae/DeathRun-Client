# DeathRun-Client# DeathRun-Client

> Unity 기반 멀티플레이 클라이언트 프로토타입 (개발 중)

## 프로젝트 소개

`DeathRun-Client`는 `DeathRun-Server`와 통신하는 Unity 클라이언트 프로젝트입니다.

처음에는 **2D 플랫포머 데스런 게임**을 목표로 기획했지만,
현재는 포트폴리오 목적에 맞춰 범위를 조정하여
다음과 같은 **네트워크 핵심 기능과 클라이언트 구조 구현**에 집중하고 있습니다.

- 서버 접속 및 세션 식별자 수신
- 로비 UI 및 채팅 흐름
- 방 생성 / 참가를 위한 구조 설계
- 게임 씬 이동을 위한 기반 구성
- 플레이어 이동/점프/애니메이션 기본 컨트롤러 구성

이 프로젝트는 완성형 게임 클라이언트보다는,
**서버와 맞물리는 멀티플레이 클라이언트 구조를 어떻게 설계하고 있는지**를 보여주기 위한 포트폴리오입니다.

---

## 개발 목적

- C++ 서버와 통신하는 Unity 클라이언트 직접 구현
- 패킷 직렬화/역직렬화 구조 설계
- 로비/게임 씬 분리와 상태 흐름 정리
- 채팅 UI, 풀링, 매니저 구조 등 재사용 가능한 클라이언트 기반 구성
- 서버 개발자 관점에서 클라이언트-서버 프로토콜을 함께 검증하기

---

## 현재 구성된 시스템

### 1. 씬 구성
현재 프로젝트에는 아래 두 개의 핵심 씬이 포함되어 있습니다.

- `LobbyScene` : 로비 및 채팅 중심 씬
- `GameScene` : 실제 플레이 씬 확장을 위한 기반 씬

### 2. 네트워크 매니저
`NetworkManager`를 중심으로 서버 연결과 패킷 처리를 관리합니다.

주요 역할은 다음과 같습니다.

- 서버 접속
- 수신 패킷 큐 관리
- 메인 스레드에서 패킷 처리
- `S_CHAT`, `E_ACCEPT` 등 패킷 분기 처리
- 서버에서 받은 `sessionId`를 플레이어 식별 정보로 반영

### 3. TCP 클라이언트 코어
`TcpClientCore`에서 실제 소켓 연결과 비동기 수신 루프를 담당합니다.

- 서버 연결 / 종료
- async read 기반 수신 루프
- 패킷 파서에 바이트 누적
- 완성된 패킷을 이벤트로 상위 계층에 전달
- disconnect / error 이벤트 분리

### 4. 패킷 계층
Unity 클라이언트에서도 서버와 동일한 방향으로
`[size][id][body]` 구조의 패킷을 처리할 수 있도록 전용 패킷 계층을 분리했습니다.

구성 요소는 다음과 같습니다.

- `PacketBuilder` : 패킷 생성
- `PacketParser` : 스트림 데이터에서 패킷 분리
- `PacketSerializer` : 기본 타입 / 고정 길이 문자열 직렬화
- `Protocol/*` : 개별 패킷 정의 (`ChatPacket`, `EAcceptPacket`, `PacketId`)

### 5. 채팅 UI
로비 채팅을 위해 `ChatUI`와 `ChatElement`를 구성했습니다.

- 입력 필드에서 메시지 전송
- 서버 수신 패킷을 UI에 반영
- 채팅 엘리먼트 프리팹을 동적으로 생성
- 풀링 구조와 연결해 UI 오브젝트 재사용 가능하도록 설계

### 6. Addressables + Pooling
UI 오브젝트 생성 비용을 줄이기 위해,
Addressables와 PoolManager를 결합한 구조를 사용하고 있습니다.
 
- `AddressableManager` : 프리팹 로드/캐시
- `PoolManager` : 오브젝트 preload / spawn / 재사용
- `IPoolable` : spawn/despawn 시점 훅 제공

단순 채팅 UI라도 생성/파괴를 반복하지 않도록 설계해,
클라이언트 구조를 더 깔끔하게 가져가려 했습니다.

### 7. 플레이어 기본 컨트롤러
`GamePlay/Player`에는 아래 기본 기능이 포함되어 있습니다.

- 좌우 이동 (`PlayerMovement`)
- 점프 (`PlayerJump`)
- 애니메이션 상태 제어 (`PlayerAnimation`)

아직 멀티플레이 동기화까지 연결된 상태는 아니지만,
이후 `R_MOVE` / `S_MOVE`와 연결하기 위한 기반으로 작성했습니다.

---

## 구현 현황

| 기능 | 상태 | 설명 |
|---|---|---|
| 서버 접속 | 구현 | `NetworkManager`에서 로컬 서버 연결 |
| 접속 승인 처리 (`E_ACCEPT`) | 구현 | 서버가 준 sessionId를 저장 |
| 채팅 UI 구조 | 구현 | 입력/수신/표시 흐름 구성 |
| 패킷 빌드/파싱 | 구현 | size + id 기반 패킷 처리 |
| 로비 씬 / 게임 씬 분리 | 구현 | 기본 씬 구조 분리 |
| 로컬 플레이어 이동/점프/애니메이션 | 구현 | 플레이 기반 컨트롤러 작성 |
| 방 생성 / 참가 UI | 진행 중 | 서버 로직과 함께 확장 예정 |
| 위치 동기화 | 진행 중 | 서버 프로토콜과 연동 예정 |
| 원격 플레이어 표시 | 예정 | sessionId 기반 오브젝트 관리 예정 |

---

## 디렉터리 구조

```text
Assets/
├─ Scenes/
│  ├─ LobbyScene.unity
│  └─ GameScene.unity
├─ Scripts/
│  ├─ Core/
│  │  ├─ Addressable/
│  │  ├─ Network/
│  │  │  └─ Packet/
│  │  └─ Singleton/
│  ├─ GamePlay/
│  │  └─ Player/
│  ├─ Managers/
│  ├─ System/
│  │  └─ Pool/
│  └─ UI/
│     └─ Chat/
└─ ...
```

---

## 실행 흐름

1. Unity에서 프로젝트 실행
2. `NetworkManager`가 서버(`127.0.0.1:7777`)에 접속
3. `E_ACCEPT` 수신 후 sessionId 저장
4. 로비 채팅 UI에서 메시지 전송
5. 서버 수신 패킷을 큐에 쌓고 메인 스레드에서 처리
6. 이후 룸/플레이 동기화 기능으로 확장 예정

---

## 기술 스택

- Engine: Unity
- Language: C#
- Network: TCP Socket
- Protocol: custom packet (`size + id + body`)
- UI: Unity UI / TextMeshPro
- Asset Management: Addressables

---

## 이 프로젝트에서 강조하고 싶은 점

이 프로젝트는 단순히 Unity 기능 구현보다,
다음 역량을 보여주기 위해 구성했습니다.

- 서버와 맞물리는 클라이언트 패킷 구조를 직접 설계한 경험
- TCP 기반 패킷 파싱/직렬화 흐름 구현 경험
- 서버 이벤트를 Unity 메인 스레드 흐름으로 넘기는 구조 설계 경험
- 채팅 UI, 매니저, 풀링 구조 등 확장 가능한 클라이언트 구조 설계 경험
- 서버 개발자 관점에서 클라이언트와 프로토콜을 함께 검증한 경험

---

## 앞으로 구현할 내용

- 룸 생성 / 참가 UI 완성
- 룸 목록 표시
- 게임 씬 전환 흐름 정리
- 원격 플레이어 생성 / 제거
- 위치 동기화 반영
- leave / disconnect 처리 UI 반영
- sessionId 기반 플레이어 오브젝트 관리

---

## 관련 프로젝트

- [DeathRun-Server](https://github.com/imutae/DeathRun-Server)
  - 이 클라이언트가 통신하는 게임 서버
- [ServerEngine.IOCP](https://github.com/imutae/ServerEngine.IOCP)
  - 서버 프로젝트의 기반 엔진

---

## 회고

서버 개발을 목표로 하고 있지만,
클라이언트까지 함께 구현해 보면서
프로토콜 설계가 실제 UI와 플레이 흐름에 어떤 영향을 주는지 더 구체적으로 이해하게 되었습니다.

특히 이 프로젝트를 통해,
“서버 패킷 하나가 실제 클라이언트 화면에서는 어떻게 소비되는가”를 직접 확인할 수 있었고,
이는 게임 서버 개발자로 성장하는 데 큰 도움이 되고 있습니다.
