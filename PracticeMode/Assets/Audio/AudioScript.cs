using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class AudioScript : MonoBehaviour
{
    public AudioClip[] clips;
    public bool random = false;
    private AudioSource AS;
    int order = 0;

    //song title
    public Text songName;


    // Start is called before the first frame update
    void Start()
    {
        AS = GetComponent<AudioSource>();
        songName.text = "Song " + order + 1;
    }

    // Update is called once per frame
    void Update()
    {
        if(!AS.isPlaying)
        {
            if(random == true)
            {
                GetRandom();
            }
            else
            {
                GetNext();
            }
        }
    }

    private void GetRandom()
    {
        order = Random.Range(0, clips.Length);
            songName.text = "Song " + (order + 1);
        AS.Stop();
        AS.clip = clips[order];
        AS.Play();
    }

    public void GetNext()
    {
        if (order >= clips.Length - 1)
            order = 0;
        else
            order += 1;
        songName.text = "Song " + (order + 1);


        AS.Stop();
        AS.clip = clips[order];
        AS.Play();
    }

    public void GetPrev()
    {
        if (order >= clips.Length - 1)
            order = 0;
        else if (order == 0)
            order = clips.Length - 1;
        else
            order -= 1;
        songName.text = "Song " + (order + 1);

        AS.Stop();
        AS.clip = clips[order];
        AS.Play();
    }
    public void OnButtonNext()
    {
        GetNext();
    }

    public void OnButtonPrev()
    {
        GetPrev();
    }
}
