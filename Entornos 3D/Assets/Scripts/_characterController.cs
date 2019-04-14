using UnityEngine;
using UnityEngine.Events;

public class _characterController : MonoBehaviour
{
    public Camera cam;
    public UnityEvent onInteractionInput;
    private InputData input;
    public AnimMovement characterMovement;

    public bool onInteractionZone { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        characterMovement = GetComponent<AnimMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        // Get input from player
        input.getInput();
        // Use jump as action button only if
        //the character is inside an InteractionZone
        if (onInteractionZone && input.jump)
        {
            onInteractionInput.Invoke();
        }
        else
            //Apply input to character
            CharacterMovement();
    }
  
    void CharacterMovement()
    {
        float h = input.Horizontal;
        float v = input.Vertical;
        Camera c = cam;

        bool j = input.jump;

        bool d = input.dash;



        characterMovement.moveCharacter(h, v, c, j, d);

    }


}
