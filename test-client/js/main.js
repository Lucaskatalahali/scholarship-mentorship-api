const adminScholarshipLink =
    document.getElementById("adminScholarshipLink");

const authButton =
    document.getElementById("authButton");

const scholarshipList =
    document.getElementById("scholarshipList");


function getTokenPayload() {

    const token = localStorage.getItem("token");

    if (!token) return null;

    try {

        const payload = token.split(".")[1];

        return JSON.parse(atob(payload));

    } catch (error) {

        console.error("Invalid token:", error);

        return null;
    }
}


function setupNavigation() {

    const payload = getTokenPayload();

    if (!payload) return;

    const role =
        payload[
            "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
        ];

    if (role === "Admin") {

        adminScholarshipLink.style.display = "block";
    }

    authButton.textContent = "Profile";
    authButton.href = "profile.html";
}


async function loadScholarships() {

    try {

        const response = await fetch(
            "http://localhost:5274/scholarships"
        );

        if (!response.ok) {

            scholarshipList.innerHTML =
                "<p>Could not load scholarships.</p>";

            return;
        }

        const scholarships = await response.json();

        if (scholarships.length === 0) {

            scholarshipList.innerHTML =
                "<p>No scholarships available.</p>";

            return;
        }

        scholarshipList.innerHTML = scholarships.map(scholarship => `
            <div class="card mb-3">

                <div class="card-body">

                    <h5 class="card-title">
                        ${scholarship.name}
                    </h5>

                    <p class="card-text">
                        ${scholarship.description ?? ""}
                    </p>

                    <p>
                        <strong>Deadline:</strong>
                        ${scholarship.deadline}
                    </p>

                    <p>
                        <strong>Eligibility:</strong>
                        ${scholarship.eligibility ?? "Not specified"}
                    </p>

                    <p>
                        <strong>How to Apply:</strong>
                        ${scholarship.howToApply ?? "Not specified"}
                    </p>

                    <p>
                        <strong>Required Documents:</strong>
                        ${scholarship.requiredDocuments ?? "Not specified"}
                    </p>

                    ${
                        scholarship.officialUrl
                            ? `
                                <a
                                    href="${scholarship.officialUrl}"
                                    target="_blank"
                                    class="btn btn-primary">
                                    Official Website
                                </a>
                              `
                            : ""
                    }

                </div>

            </div>
        `).join("");

    } catch (error) {

        console.error(error);

        scholarshipList.innerHTML =
            "<p>Could not connect to the API.</p>";
    }
}


setupNavigation();
loadScholarships();