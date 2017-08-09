using UnityEngine;

public class BlocksParentBehaviors : MonoBehaviour {

    public bool IsEditable;
    private bool defaultEditable;

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void OnHide()
    {
        MeshRenderer[] meshRenderers = this.transform.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer mr in meshRenderers)
        {
            if (mr != null)
                mr.enabled = false;
        }

        // stop audio
        AudioSource[] audioSources = this.GetComponents<AudioSource>();
        if (audioSources != null && audioSources.Length > 0)
        {
            foreach (AudioSource audioSource in audioSources)
            {
                // stop audio
                audioSource.Stop();
            }
        }

        // make not editable
        defaultEditable = IsEditable;
        IsEditable = false;
    }

    public void OnShow()
    {
        MeshRenderer[] meshRenderers = this.transform.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer mr in meshRenderers)
        {
            if (mr != null)
                mr.enabled = true;
        }

        // start audio
        AudioSource[] audioSources = this.GetComponents<AudioSource>();
        if (audioSources != null && audioSources.Length > 0)
        {
            foreach (AudioSource audioSource in audioSources)
            {
                // stop audio
                audioSource.Play();
            }
        }

        // restore default editable state
        IsEditable = defaultEditable;
    }
}
