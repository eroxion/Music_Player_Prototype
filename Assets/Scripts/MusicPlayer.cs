using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class MusicPlayer : MonoBehaviour {
    [SerializeField] private AudioClip[] songs;
    [SerializeField] private Button playButton;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button previousButton;
    [SerializeField] private Button[] songButtons;
    [SerializeField] private TMP_Text m_Title;
    [SerializeField] private TMP_Text m_Note;
    [SerializeField] private string[] notes;
    [SerializeField] private Slider seekbar;
    [SerializeField] private TMP_Text currentTimeText;
    [SerializeField] private TMP_Text totalTimeText;
    [SerializeField] private GameObject particles;
    private AudioSource audioSource;
    private int currentIndex;
    private bool isPlaying = false;
    private bool isSeeking = false;

    private void Start() {
        Application.runInBackground = true;
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        particles.SetActive(false);
        seekbar.minValue = 0;
        seekbar.maxValue = 1;

        EventTrigger trigger = seekbar.gameObject.GetComponent<EventTrigger>() ?? seekbar.gameObject.AddComponent<EventTrigger>();

        var pointerDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        pointerDown.callback.AddListener((data) => { isSeeking = true; });
        trigger.triggers.Add(pointerDown);

        var pointerUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        pointerUp.callback.AddListener((data) => { isSeeking = false; });
        trigger.triggers.Add(pointerUp);

        for (int i = 0; i < songButtons.Length; i++) {
            int index = i;
            songButtons[i].onClick.AddListener(() => PlaySong(index));
        }
        playButton.onClick.AddListener(TogglePlayPause);
        pauseButton.onClick.AddListener(TogglePlayPause);
        nextButton.onClick.AddListener(() => ShiftSong(true));
        previousButton.onClick.AddListener(() => ShiftSong(false));

        seekbar.onValueChanged.AddListener(OnSeekbarValueChanged);

        playButton.gameObject.SetActive(true);
        pauseButton.gameObject.SetActive(false);
        currentIndex = 0;
        PlaySong(currentIndex);
    }

    private void Update() {
        if (audioSource.clip == null) return;

        if (!isSeeking) {
            seekbar.value = audioSource.time / audioSource.clip.length;
        }

        currentTimeText.text = FormatTime(audioSource.time);
        totalTimeText.text = FormatTime(audioSource.clip.length);
    }

    private string FormatTime(float timeInSeconds) {
        int minutes = (int)(timeInSeconds / 60);
        int seconds = (int)(timeInSeconds % 60);
        return $"{minutes:00}:{seconds:00}";
    }

    private void PlaySong(int songIndex) {
        if (songIndex < 0 || songIndex >= songs.Length) return;

        audioSource.Stop();
        particles.SetActive(false);
        audioSource.clip = songs[songIndex];
        audioSource.Play();
        particles.SetActive(true);
        isPlaying = true;

        m_Title.text = songButtons[songIndex].GetComponentInChildren<TMP_Text>().text;
        m_Note.text = notes[songIndex];
        currentIndex = songIndex;

        playButton.gameObject.SetActive(false);
        pauseButton.gameObject.SetActive(true);
    }

    private void TogglePlayPause() {
        if (isPlaying) {
            audioSource.Pause();
            particles.SetActive(false);
        }
        else {
            audioSource.Play();
            particles.SetActive(true);
        }
        isPlaying = !isPlaying;
        playButton.gameObject.SetActive(!isPlaying);
        pauseButton.gameObject.SetActive(isPlaying);
    }

    private void ShiftSong(bool next) {
        currentIndex = (currentIndex + (next ? 1 : -1) + songs.Length) % songs.Length;
        PlaySong(currentIndex);
    }

    private void OnSeekbarValueChanged(float value) {
        if (isSeeking && audioSource.clip != null) {
            audioSource.time = value * audioSource.clip.length;
        }
    }
}