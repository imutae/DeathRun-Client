# DeathRun-Client

> Unity 기반 TCP 멀티플레이 클라이언트 포트폴리오 프로젝트

## 프로젝트 소개

`DeathRun-Client`는 별도 게임 서버와 TCP 소켓으로 통신하는 Unity 클라이언트 프로젝트입니다.

이 프로젝트는 완성된 상용 게임 클라이언트보다는, **서버와 통신하는 Unity 클라이언트 구조를 직접 설계하고 구현하는 것**에 초점을 맞추고 있습니다.  
현재는 로비, 채팅, 방 목록, 방 생성/참가, 씬 전환, 플레이어 위치 동기화의 기본 흐름을 구현하고 있으며, 서버에서 전달되는 패킷을 Unity 클라이언트에서 어떻게 처리하고 UI / 게임 오브젝트에 반영하는지를 보여주는 것을 목표로 합니다.

---

## 개발 목적

이 프로젝트에서 중점적으로 다룬 내용은 다음과 같습니다.

- Unity 클라이언트와 TCP 서버 간 직접 통신 구현
- TCP stream 기반 패킷 파싱 구조 구현
- 바이너리 패킷 직렬화 / 역직렬화 구조 설계
- 로비 UI와 서버 패킷 흐름 연결
- 방 목록, 방 생성, 방 참가 흐름 구현
- 방 참가 성공 시 게임 씬으로 전환하는 구조 구현
- `sessionId` 기반 플레이어 식별 구조 구현
- 로컬 플레이어 이동 정보 송신
- 원격 플레이어 생성, 퇴장, 위치 갱신 처리
- Addressables와 오브젝트 풀링을 활용한 프리팹 재사용 구조 구성

---

## 개발 환경

| 항목 | 내용 |
| --- | --- |
| Engine | Unity 6000.3.11f1 |
| Language | C# |
| Network | TCP Socket |
| Protocol | Custom Binary Protocol |
| UI | Unity UI, TextMeshPro |
| Input | Unity Input System |
| Asset Loading | Unity Addressables |
| Rendering | URP, 2D 기반 구성 |

---

## 주요 기능

### 1. 서버 접속

`NetworkManager`가 클라이언트 실행 시 서버에 접속합니다.

현재 서버 주소는 코드 상수로 관리됩니다.

```csharp
127.0.0.1:7777
```

서버 접속 후 수신 루프를 시작하고, 서버에서 전달되는 패킷을 큐에 저장한 뒤 Unity 메인 스레드에서 처리합니다.

### 2. 패킷 처리 구조

패킷은 다음 구조를 기준으로 처리합니다.

```text
[size: ushort][id: ushort][body: bytes]
```

| 구성 요소 | 설명 |
| --- | --- |
| `size` | header와 body를 포함한 전체 패킷 크기 |
| `id` | 패킷 식별자 |
| `body` | 패킷별 데이터 영역 |

패킷 처리는 아래 클래스로 분리되어 있습니다.

| 클래스 | 역할 |
| --- | --- |
| `PacketBuilder` | 송신용 byte 배열 생성 |
| `PacketParser` | TCP stream에서 완성된 패킷 단위로 분리 |
| `PacketSerializer` | 기본 타입과 고정 길이 문자열 직렬화 / 역직렬화 |
| `PacketRules` | 패킷 헤더 크기, 최대 패킷 크기 등 공통 규칙 관리 |
| `Protocol/*` | 패킷별 body 구조 정의 |

### 3. 세션 ID 관리

서버 접속 승인 패킷인 `E_ACCEPT`를 수신하면 서버가 발급한 `sessionId`를 저장합니다.

`PlayerManager`는 로컬 클라이언트의 `LocalSessionId`를 관리하며, 이후 채팅, 이동, 원격 플레이어 구분 등에 사용됩니다.

### 4. 로비 UI

로비에서는 다음 흐름을 처리합니다.

- 방 목록 요청
- 방 목록 표시
- 방 생성 요청
- 방 참가 요청
- 방 참가 결과 처리
- 채팅 메시지 송신 / 수신 / 표시

관련 스크립트는 다음과 같습니다.

| 스크립트 | 역할 |
| --- | --- |
| `LobbyUIManager` | 로비 UI 패널 제어 |
| `PlayPanelUI` | 방 목록 요청 / 표시 / 방 생성 버튼 처리 |
| `RoomJoinButtonHandle` | 개별 방 참가 버튼 처리 |
| `ChatUI` | 채팅 입력, 송신, 수신 메시지 표시 |
| `ChatElement` | 채팅 메시지 UI 요소 |

### 5. 게임 씬 전환

방 생성 또는 방 참가 요청 후 서버로부터 `S_JOIN` 패킷을 수신합니다.

`S_JOIN`이 성공이면 `SceneLoadManager`가 현재 방의 플레이어 세션 ID 목록을 캐싱하고 `GameScene`으로 전환합니다.  
실패한 경우에는 게임 씬으로 이동하지 않고 결과 이벤트만 전달합니다.

### 6. 플레이어 이동 동기화 기반

로컬 플레이어는 일정 주기로 자신의 위치를 서버에 전송합니다.

| 스크립트 | 역할 |
| --- | --- |
| `PlayerMovement` | 로컬 플레이어 좌우 이동 |
| `PlayerJump` | 로컬 플레이어 점프 |
| `PlayerAnimation` | 이동 / 점프 / 낙하 애니메이션 상태 처리 |
| `PlayerAsync` | 로컬 플레이어 위치 변경 감지 후 `R_MOVE` 송신 |

서버에서 다른 플레이어의 위치를 `S_MOVE`로 전달하면, `GamePlayManager`가 해당 `sessionId`의 원격 플레이어 오브젝트 위치를 갱신합니다.

### 7. 원격 플레이어 관리

`GamePlayManager`는 게임 씬에서 원격 플레이어를 관리합니다.

- 현재 방의 세션 ID 목록을 기반으로 원격 플레이어 생성
- `E_JOIN` 수신 시 새 원격 플레이어 생성
- `E_LEAVE` 수신 시 원격 플레이어 제거
- `S_MOVE` 수신 시 원격 플레이어 위치 갱신
- 로컬 플레이어의 `sessionId`는 원격 플레이어 생성 대상에서 제외

### 8. 오브젝트 풀링

`PoolManager`와 `IPoolable`을 기반으로 오브젝트 재사용 구조를 구성했습니다.

현재 풀링은 채팅 UI 요소와 원격 플레이어 프리팹 재사용에 활용됩니다.

| 구성 요소 | 역할 |
| --- | --- |
| `PoolManager` | 오브젝트 생성 / 재사용 / 반환 관리 |
| `IPoolable` | 풀링 오브젝트의 spawn / despawn 콜백 인터페이스 |
| `AddressableManager` | Addressables 기반 프리팹 로드 및 캐싱 |
| `Address` | Addressables key 상수 관리 |

---

## 현재 구현 상태

| 기능 | 상태 | 설명 |
| --- | --- | --- |
| 서버 접속 | 구현 | 클라이언트 실행 시 TCP 서버 접속 |
| 접속 승인 처리 | 구현 | `E_ACCEPT` 수신 후 `LocalSessionId` 저장 |
| 패킷 빌드 / 파싱 | 구현 | `[size][id][body]` 구조 기반 처리 |
| 패킷 크기 검증 | 구현 | `PacketRules` 기반 최소 / 최대 크기 검증 |
| 채팅 송신 / 수신 | 구현 | `R_CHAT` / `S_CHAT` 패킷 기반 처리 |
| 방 목록 요청 / 표시 | 구현 | `R_ROOM_LIST` / `S_ROOM_LIST` 패킷 기반 처리 |
| 방 생성 요청 | 구현 | invalid room id를 이용해 방 생성 요청 |
| 방 참가 요청 | 구현 | room id 기반 `R_JOIN` 송신 |
| 방 참가 결과 처리 | 구현 | `S_JOIN` 성공 시 게임 씬 전환 |
| Join 실패 처리 | 구현 | 실패 시 게임 씬으로 이동하지 않음 |
| 로비 씬 / 게임 씬 전환 | 구현 | `SceneLoadManager`에서 씬 전환 관리 |
| 로컬 플레이어 조작 | 구현 | 이동, 점프, 애니메이션 처리 |
| 로컬 위치 송신 | 구현 | `PlayerAsync`에서 주기적으로 `R_MOVE` 송신 |
| 원격 플레이어 생성 | 구현 | 방 세션 목록과 `E_JOIN` 기반 생성 |
| 원격 플레이어 퇴장 처리 | 구현 | `E_LEAVE` 기반 제거 |
| 원격 플레이어 위치 반영 | 구현 | `S_MOVE` 기반 위치 갱신 |
| 오브젝트 풀링 | 구현 | Addressables 기반 프리팹 풀링 |
| 서버 재연결 | 미구현 | 연결 실패 / 끊김 후 재접속 로직 없음 |
| 위치 보간 / 예측 | 미구현 | 현재는 수신 위치를 직접 반영 |
| 서버 주소 설정 분리 | 미구현 | 현재 코드 상수로 관리 |

---

## 주요 패킷

현재 클라이언트에서 사용하는 주요 패킷은 다음과 같습니다.

| 패킷 | 방향 | 설명 |
| --- | --- | --- |
| `E_ACCEPT` | Server → Client | 서버 접속 승인 및 `sessionId` 전달 |
| `R_CHAT` | Client → Server | 채팅 메시지 송신 |
| `S_CHAT` | Server → Client | 채팅 메시지 수신 |
| `R_ROOM_LIST` | Client → Server | 방 목록 요청 |
| `S_ROOM_LIST` | Server → Client | 방 목록 응답 |
| `R_JOIN` | Client → Server | 방 생성 또는 방 참가 요청 |
| `S_JOIN` | Server → Client | 방 생성 / 참가 결과 |
| `E_JOIN` | Server → Client | 다른 플레이어 입장 알림 |
| `N_LEAVE` | Client → Server | 클라이언트의 방 나가기 알림 |
| `E_LEAVE` | Server → Client | 다른 플레이어 퇴장 알림 |
| `R_MOVE` | Client → Server | 로컬 플레이어 위치 송신 |
| `S_MOVE` | Server → Client | 플레이어 위치 브로드캐스트 |

---

## 실행 흐름

### 로비 진입

```text
Unity 실행
→ LobbyScene 진입
→ NetworkManager 초기화
→ TCP 서버 접속
→ 수신 루프 시작
→ E_ACCEPT 수신
→ LocalSessionId 저장
```

### 로비 기능 사용

```text
방 목록 새로고침 클릭
→ R_ROOM_LIST 송신
→ S_ROOM_LIST 수신
→ PlayPanelUI가 방 목록 버튼 갱신
```

```text
방 생성 또는 방 참가 클릭
→ R_JOIN 송신
→ S_JOIN 수신
→ 성공 시 GameScene 이동
→ 실패 시 로비 유지
```

### 게임 씬 진입

```text
S_JOIN 성공
→ SceneLoadManager가 현재 방 sessionId 목록 캐싱
→ GameScene 로드
→ GamePlayManager가 원격 플레이어 생성
→ 로컬 플레이어는 위치 변경 시 R_MOVE 송신
→ S_MOVE 수신 시 원격 플레이어 위치 갱신
```

### 방 나가기

```text
ExitGame 호출
→ 원격 플레이어 오브젝트 반환
→ N_LEAVE 송신
→ LobbyScene 로드
→ 방 상태 초기화
```

---

## 디렉터리 구조

```text
Assets/
├─ Scenes/
│  ├─ LobbyScene.unity
│  └─ GameScene.unity
│
├─ Scripts/
│  ├─ Core/
│  │  ├─ Addressable/
│  │  │  ├─ Address.cs
│  │  │  └─ AddressableManager.cs
│  │  │
│  │  ├─ Network/
│  │  │  ├─ TcpClientCore.cs
│  │  │  └─ Packet/
│  │  │     ├─ Packet.cs
│  │  │     ├─ PacketBuilder.cs
│  │  │     ├─ PacketParser.cs
│  │  │     ├─ PacketSerializer.cs
│  │  │     └─ Protocol/
│  │  │
│  │  └─ Singleton/
│  │     └─ MonoSingleton.cs
│  │
│  ├─ Managers/
│  │  ├─ NetworkManager.cs
│  │  ├─ PlayerManager.cs
│  │  └─ SceneLoadManager.cs
│  │
│  ├─ UI/
│  │  ├─ LobbyUIManager.cs
│  │  ├─ PlayPanelUI.cs
│  │  ├─ RoomJoinButtonHandle.cs
│  │  └─ Chat/
│  │
│  ├─ GamePlay/
│  │  ├─ GamePlayManager.cs
│  │  ├─ Player/
│  │  └─ Trap/
│  │
│  └─ System/
│     └─ Pool/
│        ├─ PoolManager.cs
│        └─ IPoolable.cs
│
├─ Packages/
└─ ProjectSettings/
```

---

## 핵심 구조

### NetworkManager

`NetworkManager`는 네트워크 계층과 Unity 계층 사이의 중간 관리자입니다.

주요 책임은 다음과 같습니다.

- 서버 접속
- 패킷 송신
- 수신 패킷 큐 관리
- Unity 메인 스레드에서 패킷 처리
- 패킷 ID별 handler 분기
- UI / 게임플레이 계층에 이벤트 전달

### TcpClientCore

`TcpClientCore`는 실제 TCP 소켓 처리를 담당합니다.

주요 책임은 다음과 같습니다.

- 서버 연결
- 비동기 수신 루프
- 수신 byte stream을 `PacketParser`에 전달
- 완성된 패킷을 `NetworkManager`에 이벤트로 전달
- 연결 종료 / 에러 이벤트 전달

### SceneLoadManager

`SceneLoadManager`는 씬 전환과 방 상태를 관리합니다.

주요 책임은 다음과 같습니다.

- `LobbyScene` / `GameScene` 로드
- 현재 씬 타입 추적
- 방 참가 성공 시 room session id 목록 캐싱
- 로비 복귀 시 방 상태 초기화

### GamePlayManager

`GamePlayManager`는 게임 씬에서 원격 플레이어 상태를 관리합니다.

주요 책임은 다음과 같습니다.

- 현재 방의 원격 플레이어 생성
- 새 플레이어 입장 처리
- 플레이어 퇴장 처리
- 서버에서 받은 위치 패킷을 원격 플레이어 오브젝트에 반영

---

## 포트폴리오 관점에서 보여주고 싶은 점

이 프로젝트는 단순한 Unity UI 구현보다는, 서버와 실제로 통신하는 클라이언트 구조를 보여주는 데 목적이 있습니다.

특히 다음 역량을 보여주고자 했습니다.

- TCP stream을 직접 패킷 단위로 분리하는 구조 이해
- 서버와 클라이언트가 공유하는 바이너리 프로토콜 설계 경험
- 네트워크 스레드와 Unity 메인 스레드 사이의 데이터 전달 구조 설계
- 서버 패킷을 UI와 게임 오브젝트 상태로 반영하는 흐름 구현
- `sessionId` 기반 멀티플레이어 식별 구조 구현
- Addressables와 PoolManager를 활용한 프리팹 재사용 구조 구성
- 로비 → 게임 씬 → 로비 복귀까지의 클라이언트 상태 흐름 구성

---

## 현재 한계와 개선 예정

현재 구현은 멀티플레이 클라이언트의 기반 구조에 초점을 맞추고 있으며, 아래 부분은 이후 개선 대상입니다.

- 서버 주소 / 포트 설정을 코드 상수에서 설정 파일 또는 ScriptableObject로 분리
- 서버 연결 실패 / 연결 끊김에 대한 UI 처리
- 재접속 로직 추가
- 위치 동기화에 보간 / 예측 적용
- 원격 플레이어 애니메이션 동기화
- 방 참가 실패 사유를 UI에 표시
- 패킷 처리 handler 구조 분리
- 로비 UI와 게임 UI 책임 분리
- 풀링 오브젝트 생명주기 정책 정리
- 패킷 프로토콜 문서 별도 분리

---

## 실행 전 준비

이 클라이언트는 단독 실행만으로는 전체 기능을 확인할 수 없습니다.  
로비, 채팅, 방 생성, 방 참가, 이동 동기화 기능을 확인하려면 대응되는 서버가 먼저 실행되어 있어야 합니다.

현재 클라이언트는 기본적으로 아래 주소로 접속합니다.

```text
127.0.0.1:7777
```

---

## 실행 방법

1. Unity Hub에서 프로젝트를 엽니다.
2. 대응되는 서버 프로젝트를 먼저 실행합니다.
3. Unity에서 `LobbyScene`을 엽니다.
4. Play 모드로 실행합니다.
5. 서버 접속 후 로비 UI에서 채팅, 방 목록, 방 생성, 방 참가 기능을 확인합니다.
6. 방 참가 성공 시 `GameScene`으로 이동합니다.

---

## 라이선스

MIT License
