import logo from './logo.svg';
import './App.css';
import { BrowserRouter, Link, Route, Routes, useLocation, useNavigate } from 'react-router-dom';
import Login from './pages/Login';
import Register from './pages/Register';
import { Button, Layout, Menu } from 'antd';
import { Content, Header } from 'antd/es/layout/layout';
import BookList from './pages/BookList';
import Author from './pages/Author';
import BookReviews from './pages/BookReviews';
import { useEffect, useState } from 'react';
import { LogoutOutlined } from '@ant-design/icons';
import Account from './pages/Account';
import { Avatar } from 'antd';


function App() {
  const navigate = useNavigate();
  const location = useLocation();
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [userName, setUserName] = useState("");
  
  useEffect(() => {
    const token = localStorage.getItem('token');
    const userNameLocalStorage = localStorage.getItem('userName')
    const firstLetter = userNameLocalStorage ? userNameLocalStorage.charAt(0).toUpperCase() : '?';
    setUserName(firstLetter)
    setIsLoggedIn(!!token);
  }, [location]);

  const handleLogout = () => {
    localStorage.removeItem('token');//token ve id değerleri storagedan clear edildi
    localStorage.removeItem('userId');
    setIsLoggedIn(false);
    navigate('/login');
  };

  
 

  return (
    <Layout>
<Header>
  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>

  <div style={{ fontSize: '24px', fontWeight: 'bold', color: '#fff', marginRight: '40px' }}>
            <Link to="/" style={{ color: '#fff', textDecoration: 'none' }}>
              bookcommentator.com
            </Link>
          </div>
    {/* Sol Menü - Sabit Olanlar */}
    <Menu
      theme="dark"
      mode="horizontal"
      selectedKeys={[location.pathname]}
      style={{ flex: 1 }}
    >
      <Menu.Item key="/books">
        <Link to="/books">Kitaplar</Link>
      </Menu.Item>
      <Menu.Item key="/authors">
        <Link to="/authors">Yazarlar</Link>
      </Menu.Item>
      {isLoggedIn && (
        <Menu.Item key="/account">
          <Link to="/account">Hesabım</Link>
        </Menu.Item>
      )}
    </Menu>

    {/* Sağ Menü - Giriş, Çıkış ve Avatar */}
    <div style={{ display: 'flex', alignItems: 'center', gap: '16px' }}>
      {isLoggedIn ? (
        <>
          <Avatar style={{ backgroundColor: '#1890ff' }}>{userName}</Avatar>
          <Button
            type="text"
            icon={<LogoutOutlined style={{ fontSize: '20px', color: 'white' }} />}
            onClick={handleLogout}
            title="Çıkış Yap"
          />
        </>
      ) : (
        <Menu theme="dark" mode="horizontal" selectedKeys={[location.pathname]}>
          <Menu.Item key="/login">
            <Link to="/login">Giriş Yap</Link>
          </Menu.Item>
          <Menu.Item key="/register">
            <Link to="/register">Kayıt Ol</Link>
          </Menu.Item>
        </Menu>
      )}
    </div>

  </div>
</Header>


      <Content style={{ padding: '20px', minHeight: 'calc(100vh - 64px)' }}>
        <Routes>
          <Route path="/account" element={<Account />} />
          <Route path="/books" element={<BookList />} />
          <Route path="/authors" element={<Author />} />
          <Route path="/books/:bookId/reviews" element={<BookReviews />} />
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register />} />
          
        </Routes>
      </Content>
    </Layout>
  );
}


export default App;
