import React, { useState, useEffect } from 'react';
import './App.css';

function App() {
    const [repoOwner, setRepoOwner] = useState('');
    const [repoName, setRepoName] = useState('');
    const [searchQuery, setSearchQuery] = useState('');
    const [forecasts, setForecasts] = useState([]);
    const [loading, setLoading] = useState(false);
    //const [searchTimeout, setSearchTimeout] = useState(null);
    const [error, setError] = useState(null);
    const [formValid, setFormValid] = useState(false); // New state to track form validity
    const [searchEndpoint, setSearchEndpoint] = useState('search');
    const[dataSource, setDataSourse] = useState('');

    useEffect(() => {
        // Update form validity based on Repo Owner and Repo Name fields
        setFormValid(repoOwner.trim() !== '' && repoName.trim() !== '');
    }, [repoOwner, repoName]);

    const handleGetCommits = async () => {
        setLoading(true);
        setError(null);
        setDataSourse('repo')
        try {
           
            const response = await fetch(`api/Commits/Comments?repoName=${repoName}&repoOwner=${repoOwner}`);
            if (response.ok) {
                const data = await response.json();
                setForecasts(data);
                setSearchEndpoint('search');
            } else {
                setError('Failed to fetch commits.');
                setForecasts([]);
            }
        } catch (error) {
            setError('Error fetching data.');
            setForecasts([]);
        } finally {
            setLoading(false);
        }
    };

    const handleSavedCommits = async () => {
        setLoading(true);
        setError(null);
        setDataSourse('saved')
        try {
            const response = await fetch(`api/Commits/saved-commits`);
            if (response.ok) {
                const data = await response.json();
                setForecasts(data);
                setSearchEndpoint('search-saved-commits');
                setSearchQuery('');
            } else {
                setError('Failed to fetch commits.');
                setForecasts([]);
                setSearchQuery('');
            }
        } catch (error) {
            setError('Error fetching data.');
            setForecasts([]);
            setSearchQuery('');
        } finally {
            setLoading(false);
        }
    };


    const handleGetAnotherRepo = () => {
        setRepoOwner('');
        setRepoName('');
        setSearchQuery('');
        setForecasts([]);
        setError(null);
        setDataSourse('')
    };


    const handleSearchInputChange = async (e) => {
        const newSearchQuery = e.target.value;

        setSearchQuery(newSearchQuery);
        setLoading(true); // Set loading to true for immediate feedback

        try {
            let apiUrl = '';
            if (searchEndpoint === 'search') {
                apiUrl = `api/Commits/search?commitId=&repoName=${repoName}&repoOwner=${repoOwner}&message=${newSearchQuery}`;
            } else {
                apiUrl = `api/Commits/search-saved-commits?searchQuery=${newSearchQuery}`;
            }

            const response = await fetch(apiUrl);

            if (response.ok) {
                const data = await response.json();
                setForecasts(data);
            } else {
                console.error('Failed to fetch search results.');
            }
        } catch (error) {
            console.error('Error fetching data:', error);
        } finally {
            setLoading(false); // Set loading to false after the API call completes
        }
    };


    const handleSaveCommit = async (commitId, starred, repoName, repoOwner) => {
        const endpoint = starred ? 'remove-commit' : 'save-commit';
        try {
            const response = await fetch(`api/Commits/${endpoint}?commitId=${commitId}&repoName=${repoName}&repoOwner=${repoOwner}`);
            if (response.ok) {
                if (dataSource === 'repo') {
                    handleGetCommits();
                }
                else {
                    handleSavedCommits();
                }

               
            } else {
                setError(`Failed to ${starred ? 'remove' : 'save'} commit.`);
            }
        } catch (error) {
            setError('Error fetching or parsing data.');
        }
    };

    const renderSaveButton = (forecast) => {
        return (
            <button onClick={() => handleSaveCommit(forecast.commitId, forecast.starred, forecast.repoName, forecast.repoOwner)}>
                {forecast.starred ? 'Unsave' : 'Save'}
            </button>
        );
    };

    return (
        <div>
            <div >
               
                <h1>GitHub Commits</h1>
                <p>
                    Discover the latest GitHub commits effortlessly with our intuitive program. Seamlessly fetching commit data from the server, it keeps you updated on project progress, ensuring you're always in the loop.
                </p>
            </div>

            <div className = "input-wrapper">

                <div className ="form-container">
                    <div className="input-container">
                        <label>Repo Owner:</label>

                        <input type="text" value={repoOwner} onChange={(e) => setRepoOwner(e.target.value)} disabled={loading} />
                    </div>
                    <div className="input-container">
                        <label>Repo Name:</label>
                        <input type="text" value={repoName} onChange={(e) => setRepoName(e.target.value)} disabled={loading} />
                    </div>

                    <div className="form-buttons">

                        <div onClick={handleGetCommits} disabled={loading || !formValid}>
                            {loading ? 'Loading...' : 'Fetch Repo Commits'}
                        </div>
                        <div onClick={handleSavedCommits} disabled={loading || !formValid}>
                            {loading ? 'Loading...' : 'Fetch saved Commits'}
                        </div>
                        <div onClick={handleGetAnotherRepo} disabled={loading || !formValid}>
                            Clear Form
                        </div>

                    </div>
                    
                </div>
                {forecasts.length > 0 && (

                    <div className="search-container">
                        <label>Search:</label>
                        <input  type="text" value={searchQuery} onChange={handleSearchInputChange} />
                    </div>


                )}
                    

                
            </div>
            {forecasts.length === 0 && !loading && !error && (
                <p>
                    No commits found. Please enter search criteria above and click "Search Commits" to retrieve commits.
                </p>
            )}
            {forecasts.length > 0 && (
                <>
                    
                    <table className="table table-striped" aria-labelledby="tabelLabel">
                        <thead>
                            <tr>
                                <th>Commit Author</th>
                                <th>Commit Message</th>
                                <th>Commit Date</th>
                                <th>Starred Time</th>
                                <th>Action</th>
                            </tr>
                        </thead>
                        <tbody>
                            {forecasts.map((forecast) => (
                                <tr key={forecast.commitId}>
                                    <td>{forecast.commitAuthor}</td>
                                    <td>{forecast.commitMessage}</td>
                                    <td>{forecast.commitDate}</td>
                                    <td>{forecast.starredTime}</td>
                                    <td>{renderSaveButton(forecast)}</td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </>
            )}
            {error && <p>Error: {error}</p>}
        </div>
    );
}

export default App;