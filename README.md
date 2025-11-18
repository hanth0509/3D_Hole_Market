# 📖 Game Hole Market - Documentation

## 🎯 **Giới thiệu**
Game Hole Market là game 3D được phát triển bằng Unity, với hệ thống level động được tạo từ file JSON.

## 📁 **Cấu trúc dự án**

```
Assets/
├── 📂 Scripts/
│   ├── 📂 Manager/
│   │   ├── MasterData.cs       # Quản lý dữ liệu game
│   │   └── Manager.cs          # Game manager
│   ├── 📂 Level/
│   │   ├── LevelGenerator.cs   # Tạo và spawn level từ JSON
│   │   ├── PanelSwitcher.cs    # Chuyển đổi UI panels
│   │   └── SceneLoader.cs      # Quản lý scene
│   ├── 📂 Hole/
│   │   ├── Hole.cs            # Logic hole (vật thể chính)
│   │   ├── Move.cs            # Di chuyển
│   │   └── Destroy.cs         # Hủy vật thể
│   └── 📂 UI/
│       └── UILevelButton.cs   # Button chọn level
├── 📂 Resources/
│   └── 📂 Levels/
│       ├── level_1.json       # Dữ liệu level 1
│       ├── level_2.json       # Dữ liệu level 2
│       └── ... (48 levels)
├── 📂 Scenes/
│   ├── Home.unity            # Scene menu chính
│   ├── Level.unity           # Scene chọn level
│   └── GamePlay.unity        # Scene gameplay
└── 📂 Plugins/
    └── Easy Save 3/          # Plugin lưu trữ dữ liệu
```

## 🚀 **Cách thức hoạt động**

### **1. Hệ thống Level**
- **48 levels** được chia thành **4 trang**, mỗi trang **12 levels**
- Dữ liệu level được lưu trong file JSON tại `Resources/Levels/`
- Mỗi level chứa thông tin về các nhóm object và vị trí spawn

### **2. Luồng game**
```
Home Screen → Level Select → Chọn Level → Load JSON → Spawn Objects → Gameplay
```

### **3. File JSON Structure**
```json
{
    "LevelData": {
        "__type": "LevelData,Assembly-CSharp",
        "value": {
            "levelIndex": 1,
            "groups": [
                {
                    "groupName": "Group_0",
                    "positions": [
                        {"x": 0, "y": 0, "z": 0},
                        {"x": 1.5, "y": 0, "z": 0}
                    ]
                }
            ]
        }
    }
}
```

## 🛠️ **Hướng dẫn sử dụng**

### **Tạo level mới**
1. Chọn **LevelGenerator** object trong scene
2. Trong Inspector, click **Context Menu → Generate & Save Level**
3. File JSON sẽ được tạo trong `Resources/Levels/`

### **Test level**
1. Chạy game
2. Vào scene **Level Select**
3. Click button level muốn test
4. Objects sẽ được spawn theo dữ liệu JSON

### **Sử dụng Prefab**
1. Tạo prefab trong Project window
2. Chọn **LevelGenerator** object
3. Kéo prefab vào field **Object Prefab** trong Inspector
4. Khi spawn level, prefab sẽ được sử dụng thay vì cube mặc định

## 📝 **Class quan trọng**

### **MasterData.cs**
- Quản lý dữ liệu game toàn cục
- Load level data từ JSON
- Lưu trữ progress với Easy Save 3

### **LevelGenerator.cs**
- Tạo dữ liệu level ngẫu nhiên
- Spawn objects từ JSON data
- Hỗ trợ cả cube mặc định và custom prefab

### **UILevelButton.cs**
- Hiển thị button level trong Level Select
- Load level data khi được click
- Hiển thị trạng thái (mở/khóa, sao, high score)

## 🔧 **Troubleshooting**

### **Không thấy objects sau khi spawn**
- Kiểm tra Console để xem lỗi
- Đảm bảo MasterData và LevelGenerator có trong scene
- Kiểm tra file JSON có trong `Resources/Levels/`

### **Level Index hiển thị 0**
- File JSON có thể bị lỗi parse
- Kiểm tra structure JSON có đúng format không

### **Button không hoạt động**
- Đảm bảo UILevelButton được gán đúng levelIndex
- Kiểm tra OnClick event trong Button component

## 🎮 **Input System**
- Sử dụng Unity's New Input System
- File config: `InputSystem_Actions.inputactions`
- Hỗ trợ multiple input devices

## 📦 **Dependencies**
- **Unity 2022.3+**
- **Easy Save 3** - Lưu trữ dữ liệu
- **TextMesh Pro** - Hiển thị text
- **Universal Render Pipeline** - Rendering


