const loginForm = document.getElementById("loginForm");
const message = document.getElementById("message");

loginForm.addEventListener("submit", async (event) => {

    event.preventDefault();

    const email = document.getElementById("email").value;
    const password = document.getElementById("password").value;

    try {

        const response = await fetch(
            "http://localhost:5274/users/login",
            {
                method: "POST",

                headers: {
                    "Content-Type": "application/json"
                },

                body: JSON.stringify({
                    email: email,
                    password: password
                })
            }
        );


        if (response.ok) {

            const data = await response.json();

            // Save JWT
            localStorage.setItem("token", data.token);

            showMessage(
                "Login successful!",
                "success"
            );

            // Temporary redirect for testing
            setTimeout(() => {
                window.location.href = "index.html";
            }, 800);

            return;
        }


        if (response.status === 401) {

            showMessage(
                "Invalid email or password.",
                "danger"
            );

            return;
        }


        const error = await response.json();

        showMessage(
            error.title ?? "Login failed.",
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