# Hello Tap

Практическая работа № 1 по дисциплине «Программирование мобильных игр».
Проект: HelloTap_Skachkov41. Unity 6000.6.3f1, URP 2D, Android.

## Запуск

Откройте проект в Unity Hub, затем сцену `Assets/Scenes/HelloTap.unity` и нажмите Play.
Кнопка «Тапни меня» увеличивает счёт; «Сброс» обнуляет его.
Цвет счёта меняется каждые 10 нажатий. Счёт сохраняется в PlayerPrefs при
сворачивании, потере фокуса и выходе из приложения и восстанавливается при старте.

## Сборка

Установите модуль Android Build Support с SDK, NDK и OpenJDK для указанной версии Unity.
В редакторе выберите `Hello Tap > Build Android APK`.
Результат: `Builds/Android/HelloTap_Skachkov41.apk`.
Настройки: Android 8.0+, ARM64 и ARMv7, IL2CPP, портретная ориентация,
идентификатор `com.skachkov41.hellotap`, версия 1.0.0.

Для командной строки при закрытом редакторе:

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.6.3f1\Editor\Unity.exe' -batchmode -quit -projectPath $PWD -buildTarget Android -executeMethod HelloTapBuilder.BuildAndroid -logFile Builds/android-build.log
```

## Устройство и сдача

Включите USB Debugging на Android и подтвердите доверие компьютеру.
Установите APK командой `adb install -r Builds/Android/HelloTap_Skachkov41.apk`
или через File > Build Profiles > Android > Build And Run.
Проверьте нажатия, сброс, сворачивание и повторный запуск. Сделайте скриншот на телефоне.
Для сдачи нужны APK, исходный проект, отчёт на 1–2 страницы и скриншот устройства.
Скриншоты из `Builds/Verification` показывают рендер Unity, а не физический телефон.

## Проверка и структура

`Hello Tap > Verify Scene` проверяет клики, сброс, смену цвета, сохранение и крайние
значения счёта, создаёт изображения в трёх разрешениях. Проверка открывает сцену HelloTap;
сохраните текущую сцену перед её запуском.

- `Assets/Scripts/TapCounter.cs`: логика и сохранение счёта.
- `Assets/Scripts/SafeArea.cs`: отступы для вырезов и системных панелей.
- `Assets/Editor/HelloTapBuilder.cs`: создание сцены и сборка APK.
- `Assets/Editor/HelloTapVerification.cs`: проверка связей UI и поведения.

Canvas Scaler: Scale With Screen Size, 1080 × 1920, Match 0.5.
Используются TextMeshPro с кириллицей и Input System UI Input Module.
Исходные файлы и `.meta` нужно хранить в Git; временные файлы Unity исключены через `.gitignore`.
