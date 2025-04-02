import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import WebApp from "@twa-dev/sdk";
import { BackButton } from '@twa-dev/sdk/react';
import { handleGetInfoForAllUsers } from '../../api-integrations/ClientInfoAPI';
import { ClientInfo, ClientGetAll } from "../../api-integrations/Interfaces/API_Interfaces";
import '../../styles/AppStyle.sass'

const UsersPage: React.FC = () => {
    const [isMobile, setIsMobile] = useState<boolean>(false);
    const [users, setUsers] = useState<ClientInfo[]>([]);
    const [loading, setLoading] = useState<boolean>(true);
    const [page, setPage] = useState<number>(0);
    const [error, setError] = useState<string | null>(null);
    const navigate = useNavigate();

    useEffect(() => {
        WebApp.setHeaderColor('#EAEAEA');
        WebApp.setBackgroundColor('#004681');
        
        if (WebApp.platform === 'ios' || WebApp.platform === 'android') {
            setIsMobile(true);
        }
        
        WebApp.ready();
    }, []);

    const fetchUsers = async () => {
        setLoading(true);
        setError(null);
        
        try {
            const usersData = await handleGetInfoForAllUsers(page * 10, 10);
            
            if (usersData) {
                const usersArray = usersData.content || [];
                setUsers(usersArray);
                
                if (usersArray.length === 0) {
                    setError("Список пользователей пуст");
                }
            } else {
                setError("Не удалось загрузить данные");
            }
        } catch (err) {
            console.error("Fetch error:", err);
            setError(err instanceof Error ? err.message : "Неизвестная ошибка");
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchUsers();
    }, [page]);

    const handlePrevPage = () => setPage(p => Math.max(0, p - 1));
    const handleNextPage = () => setPage(p => p + 1);

    return (
        <>
            <BackButton onClick={() => navigate(-1)} />
            <div className="app_background_area">
                <div className="app_layout_area" style={isMobile ? { marginTop: '100px' } : {}}>
                    <div className="users-page">
                        <h2>Список пользователей</h2>
                        
                        {loading ? (
                            <div className="loading">Загрузка...</div>
                        ) : error ? (
                            <div className="error">{error}</div>
                        ) : (
                            <>
                                <div className="users-list">
                                    {users.length > 0 ? (
                                        users.map(user => (
                                            <div key={user.id} className="user-card">
                                                <img 
                                                    src={user.photo_url || '/default-avatar.png'} 
                                                    alt="Аватар" 
                                                    className="user-avatar" 
                                                />
                                                <div className="user-info">
                                                    <h3>{user.first_name} {user.last_name}</h3>
                                                    <p>@{user.username}</p>
                                                    <p>Заказов: {user.orders_count}</p>
                                                    <p>Баланс: {user.money_value} ₽</p>
                                                    <p>Роли: {user.roles?.join(', ') || 'нет ролей'}</p>
                                                </div>
                                            </div>
                                        ))
                                    ) : (
                                        <div className="no-users">Пользователи не найдены</div>
                                    )}
                                </div>
                                
                                <div className="pagination">
                                    <button 
                                        onClick={handlePrevPage} 
                                        disabled={page === 0}
                                        className="pagination-button"
                                    >
                                        Назад
                                    </button>
                                    <span>Страница {page + 1}</span>
                                    <button 
                                        onClick={handleNextPage} 
                                        disabled={users.length < 10}
                                        className="pagination-button"
                                    >
                                        Вперед
                                    </button>
                                </div>
                            </>
                        )}
                    </div>
                    
                    {isMobile && (
                        <div className="app_mobile_footer" style={{ zIndex: '15' }}>
                            Симбир Еда
                        </div>
                    )}
                </div>
            </div>
        </>
    );
};

export default UsersPage;