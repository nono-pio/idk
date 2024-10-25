﻿function Header() {
    return (
        <header className="w-100 d-flex flex-wrap justify-content-between gap-5 py-3 mb-4 border-bottom">
            <a href="/" className="d-flex align-items-center link-body-emphasis text-decoration-none ms-3">
                <svg className="bi me-2" width="40" height="32">
                </svg>
                <span className="fs-4">Math CAS</span>
            </a>

            <ul className="nav nav-pills me-3">
                <li className="nav-item"><a href="/" className="nav-link active" aria-current="page">Home</a></li>
                <li className="nav-item"><a href="/swagger" className="nav-link">Swagger</a></li>
            </ul>
        </header>
    )
}

export default Header;