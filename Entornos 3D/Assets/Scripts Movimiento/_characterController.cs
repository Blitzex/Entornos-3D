using UnityEngine;

public class _characterController : MonoBehaviour
{
    public Camera cam;
    private InputData input;
    private AnimMovement characterMovement;

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

        //Apply input to character
        CharacterMovement();
    }
  
    void CharacterMovement()
    {
        characterMovement.moveCharacter(input.Horizontal, input.Vertical, cam, input.jump, input.dash);

    }


}
