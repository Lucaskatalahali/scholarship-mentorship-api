const profile = document.getElementById("profile");
const message = document.getElementById("message");

async function loadProfile() {

    const token = localStorage.getItem("token");

    if (!token) {

        showMessage(
            "You are not logged in.",
            "danger"
        );

        return;
    }

    try {

        const response = await fetch(
            "http://localhost:5274/users/me",
            {
                method: "GET",

                headers: {
                    "Authorization": `Bearer ${token}`
                }
            }
        );


        if (response.ok) {

            const user = await response.json();

            profile.innerHTML = `
                <div class="mb-3">
                    <strong>Name</strong>
                    <div>${user.name}</div>
                </div>

                <div class="mb-3">
                    <strong>Email</strong>
                    <div>${user.email}</div>
                </div>

                <div class="mb-3">
                    <strong>GPA</strong>
                    <div>${user.gpa ?? "Not provided"}</div>
                </div>

                <div class="mb-3">
                    <strong>Birth Date</strong>
                    <div>${user.birthDate ?? "Not provided"}</div>
                </div>
            `;

            return;
        }


        if (response.status === 401) {

            showMessage(
                "Your session is invalid or has expired.",
                "danger"
            );

            localStorage.removeItem("token");

            return;
        }


        showMessage(
            "Could not load your profile.",
            "danger"
        );

    } catch (error) {

        console.error(error);

        showMessage(
            "Could not connect to the API.",
            "danger"
        );
    }
}


function showMessage(text, type) {

    message.innerHTML = `
        <div class="alert alert-${type}">
            ${text}
        </div>
    `;
}


loadProfile();