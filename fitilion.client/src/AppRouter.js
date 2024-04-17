import { GitHubCommit } from "./components/GitHubCommit";
import { Home } from "./components/Home";

const AppRoutes = [
    {
        index: true,
        element: <Home />
    },
    {
        path: '/githubcommit',
        element: <GitHubCommit />
    }
];

export default AppRoutes;
