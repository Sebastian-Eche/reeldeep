using UnityEngine;

public class Rod : MonoBehaviour
{
    public Animator animator;
    public GameObject hook;
    public Transform targetPosition;
    private bool animationComplete = false;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !animationComplete){
            animator.SetTrigger("Cast");
        }
        if (animationComplete){
            SettingUpHook();
        }
    }

    public void SettingUpHook(){
        hook.transform.position = Vector3.MoveTowards(hook.transform.position, targetPosition.position, 2f * Time.deltaTime);
        hook.transform.localScale = Vector3.Lerp(hook.transform.localScale, Vector3.one, 2f * Time.deltaTime);

        if ((hook.transform.position - targetPosition.transform.position).magnitude <= 0.01){
            hook.GetComponent<HookController>().enabled = true;
            this.enabled = false;
        }
    }

    public void AnimationComplete(){
        animationComplete = true;
    }
}
