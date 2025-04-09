import React, { useEffect, useState } from "react";
import {
  Card,
  Row,
  Col,
  Rate,
  Typography,
  message,
  Modal,
  Form,
  Input,
  Button,
  Upload,
  Select,
  Pagination,
  Alert
} from "antd";
import axios from "axios";
import { Link } from "react-router-dom";
import { PlusOutlined, UploadOutlined, SearchOutlined } from "@ant-design/icons";
import api from "../services/api";

const { Title, Paragraph } = Typography;
const { Option } = Select;

const BookList = () => {
  const [books, setBooks] = useState([]);
  const [authors, setAuthors] = useState([]);
  const [createModalVisible, setCreateModalVisible] = useState(false);
  const [alertInfo, setAlertInfo] = useState({ visible: false, message: '', type: '' });
  const [form] = Form.useForm();
  const [filterForm] = Form.useForm();
  const [imageFile, setImageFile] = useState(null);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(8);

  const handleFilter = async (values) => {
    try {
      // Örnek request: { title: "kitAp", type: "Fiction", sortRating: "desc" }
      const res = await api.get("/books/book-filter", {
        params: values,
      });
      setBooks(res.data);
    } catch (error) {
      message.error("Filtreleme ile kitaplar yüklenemedi");
    }
  };


  const fetchBooks = async () => {
    try {
      const res = await api.get("/books"); // baseURL + /books => http://localhost:5000/api/v1/books
      setBooks(res.data);
    } catch (error) {
      message.error("Kitaplar yüklenemedi");
    }
  };


  const fetchAuthors = async () => {
    try {
      const res = await api.get("/authors");
      setAuthors(res.data);
    } catch (error) {
      message.error("Yazarlar yüklenemedi");
    }
  };

  useEffect(() => {
    fetchBooks();
    fetchAuthors();
  }, []);

  const handleCreateBook = async (values) => {
    
    const formData = new FormData();
    formData.append("title", values.title);
    formData.append("type", values.type);
    formData.append("description", values.description);
    formData.append("imageUrl", values.imageUrl);
    formData.append("authorId", values.authorId);
    formData.append("userId", localStorage.getItem("userId"));
    if (values.imageFile && values.imageFile.length > 0) {
      const file = values.imageFile[0].originFileObj;
      formData.append("imageFile", file);
    }
    
    try {
      await axios.post(
        "http://localhost:5217/api/v1/books/create-book",
        formData,{
            headers: {
              Authorization: `Bearer ${localStorage.getItem("token")}`
            }
          }
      );
      setAlertInfo({ visible: true, message: "Kitap başarıyla eklendi", type: "success" });
      setCreateModalVisible(false);
      form.resetFields();
      setImageFile(null);
      fetchBooks();
    } catch (err) {
        setAlertInfo({ visible: true, message: "Kitap zaten sistemde mevcut", type: "info" });
    }
  };

  const handleImageChange = (info) => {
    if (info.file.status !== "uploading") {
      setImageFile(info.file.originFileObj);
    }
  };

  const indexOfLastBook = currentPage * pageSize;
  const indexOfFirstBook = indexOfLastBook - pageSize;
  const currentBooks = books.slice(indexOfFirstBook, indexOfLastBook);


  return (
    <div style={{ padding: "20px" }}>
        {alertInfo.visible && (
        <Alert 
          message={alertInfo.message} 
          type={alertInfo.type} 
          showIcon 
          closable 
          onClose={() => setAlertInfo({ ...alertInfo, visible: false })}
          style={{ marginBottom: "20px" }}
        />
      )}
      <div
        style={{
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
        }}
      >
                <Form
          form={filterForm}
          layout="inline"
          onFinish={handleFilter}
          style={{ marginBottom: 16 }}
        >
          <Form.Item name="title" label="Kitap Adı">
            <Input placeholder="Kitap adı giriniz" allowClear />
          </Form.Item>
          <Form.Item name="type" label="Tür">
            <Select placeholder="Tür seçiniz" allowClear style={{ width: 150 }}>
              <Option value="Fiction">Kurgu</Option>
              <Option value="NonFiction">Kurgu Olmayan</Option>
              <Option value="Science">Bilim</Option>
              <Option value="History">Tarih</Option>
              <Option value="Fantasy">Fantezi</Option>
              <Option value="Biography">Biyografi</Option>
              <Option value="Other">Diğer</Option>
            </Select>
          </Form.Item>
          <Form.Item name="sortRating" label="Rating Sıralama">
            <Select placeholder="Sıralama" allowClear style={{ width: 150 }}>
              <Option value="asc">Artan</Option>
              <Option value="desc">Azalan</Option>
            </Select>
          </Form.Item>
          <Form.Item>
            <Button type="primary" htmlType="submit">
              Ara
            </Button>
          </Form.Item>
          <Form.Item>
            <Button onClick={fetchBooks}>Temizle</Button>
          </Form.Item>
        </Form>
        {/* <Title level={2}></Title> */}
        {/* bu kısma arama butonları filtreleme vs. eklenecek */}
        {localStorage.getItem("token") && <Button 
          type="primary"
          icon={<PlusOutlined />}
          onClick={() => setCreateModalVisible(true)}
        >
          Kitap Ekle
        </Button>}
       
      </div>
      <Title level={2} style={{ textAlign: "center" }}>
        Kitap Listesi
      </Title>
<Row gutter={[16, 16]} justify="center" align="stretch">
  {currentBooks.map((book) => (
    <Col xs={24} sm={12} md={8} lg={5} key={book.id}>
      <Card
        hoverable
        style={{ height: '100%', display: 'flex', flexDirection: 'column', justifyContent: 'space-between' }}
        cover={
          <img
            alt={book.title}
            src={
              book.imageFile
                ? `http://localhost:5217${book.imageFile}`
                : book.imageUrl
            }
            style={{ height: 340, objectFit: 'cover' }}
          />
        }
      >
        <Title level={4}>{book.title}</Title>
        <Paragraph type="secondary">{book.type}</Paragraph>
        <Paragraph ellipsis={{ rows: 3 }} style={{ minHeight: '72px' }}>
          {book.description}
        </Paragraph>
        <div
                style={{
                  display: "flex",
                  alignItems: "center",
                  justifyContent: "space-between",
                }}
              >
                <div
                  style={{ display: "flex", alignItems: "center", gap: "8px" }}
                >
                  <Rate allowHalf disabled defaultValue={book.rating} />
                  <span>{book.rating.toFixed(1)}</span>
                </div>
                {localStorage.getItem("token") && (
                  <Link to={`/books/${book.id}/reviews`}>İncelemeler</Link>
                )}
              </div>
      </Card>
    </Col>
  ))}
</Row>
<div style={{ textAlign: "center", marginTop: "20px" }}>
        <Pagination
          simple
          current={currentPage}
          pageSize={pageSize}
          total={books.length}
          onChange={(page, size) => {
            setCurrentPage(page);
            setPageSize(size);
          }}
        />
      </div>
      <Modal
        open={createModalVisible}
        title="Yeni Kitap Ekle"
        onCancel={() => setCreateModalVisible(false)}
        footer={null}
      >
        <Form form={form} layout="vertical" onFinish={handleCreateBook}>
          <Form.Item
            name="title"
            rules={[{ required: true, message: "Lütfen başlık giriniz" }]}
            label="Kitap Adı"
          >
            <Input />
          </Form.Item>
          <Form.Item name="description" label="Açıklama">
            <Input />
          </Form.Item>
          <Form.Item
            name="type"
            label="Tür"
            rules={[{ required: true, message: "Lütfen tür seçiniz" }]}
          >
            <Select placeholder="Tür seçiniz">
              <Option value="Fiction">Kurgu</Option>
              <Option value="NonFiction">Kurgu olmayan</Option>
              <Option value="Science">Bilim</Option>
              <Option value="History">Tarih</Option>
              <Option value="Fantasy">Fantezi</Option>
              <Option value="Biography">Biyografi</Option>
              <Option value="Other">Diğer</Option>
            </Select>
          </Form.Item>
          <Form.Item name="imageUrl" label="Resim URL">
            {" "}
            <Input />{" "}
          </Form.Item>
          <Form.Item
            name="imageFile"
            label="Resim Dosyası"
            valuePropName="fileList"
            getValueFromEvent={(e) => e.fileList}
          >
            <Upload beforeUpload={() => false} maxCount={1}>
              <Button icon={<UploadOutlined />}>Resim Yükle</Button>
            </Upload>
          </Form.Item>
          <Form.Item
            name="authorId"
            label="Yazar Seçimi"
            rules={[{ required: true, message: "Yazar seçiniz" }]}
          >
            <Select
              placeholder="Yazar seçiniz"
              showSearch
              optionFilterProp="children"
              filterOption={(input, option) =>
                option.children.toLowerCase().indexOf(input.toLowerCase()) >= 0
              }
              suffixIcon={<SearchOutlined />}
            >
              {authors.map((author) => (
                <Option key={author.id} value={author.id}>
                  {author.name}
                </Option>
              ))}
            </Select>
          </Form.Item>

          <Button type="primary" htmlType="submit">
            Ekle
          </Button>
        </Form>
      </Modal>
    </div>
  );
};

export default BookList;
