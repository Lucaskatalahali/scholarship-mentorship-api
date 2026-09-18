const registerForm = document.getElementById("registerForm");
const message = document.getElementById("message");

registerForm.addEventListener("submit", async (event) => {

    event.preventDefault();

    const user = {
        name: document.getElementById("name").value,
        email: document.getElementById("email").value,
        password: document.getElementById("password").value,
        gpa: Number(document.getElementById("gpa").value),
        birthDate: document.getElementById("birthDate").value
    };

    try {

        const response = await fetch(
            "http://localhost:5274/users",
            {
                method: "POST",

                headers: {
                    "Content-Type": "application/json"
                },

                body: JSON.stringify(user)
            }
        );


        if (response.ok) {

            showMessage(
                "Account created successfully!",
                "success"
            );

            registerForm.reset();

            setTimeout(() => {
                window.location.href = "login.html";
            }, 1000);

            return;
        }


if (response.status === 400) {

    const error = await response.json();

    console.log(error);

    showMessage(
        JSON.stringify(error.errors),
        "danger"
    );

    return;
}


        showMessage(
            "Something went wrong.",
            "danger"
        );

    } catch (error) {

        console.error(error);

        showMessage(
            "Could not connect to the API.",
            "danger"
        );
    }
});


function showMessage(text, type) {

    message.innerHTML = `
        <div class="alert alert-${type}">
            ${text}
        </div>
    `;
}