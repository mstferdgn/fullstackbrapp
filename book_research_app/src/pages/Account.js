import React, { useEffect, useState } from "react";
import {
  Card,
  Row,
  Col,
  Typography,
  Modal,
  Form,
  Input,
  Button,
  Select,
  message,
  Upload,
} from "antd";
import axios from "axios";
import { EditOutlined, DeleteOutlined, UploadOutlined} from "@ant-design/icons";
import api from "../services/api";

const { Title, Paragraph } = Typography;
const { Option } = Select;

const Account = () => {
  const [books, setBooks] = useState([]);
  const [editingBook, setEditingBook] = useState(null);
  const [form] = Form.useForm();

  const fetchUserBooks = async () => {
    try {
      const res = await api.get("/books/my-books", {
        headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
      });
      setBooks(res.data);
    } catch (error) {
      message.error("Kitaplar yüklenemedi");
    }
  };

  useEffect(() => {
    fetchUserBooks();
  }, []);

  // 🟡 FORM ALANLARINI DOLDUR
  useEffect(() => {
    if (editingBook) {
      form.setFieldsValue({
        title: editingBook.title,
        description: editingBook.description,
        type: editingBook.type,
        imageUrl: "",

      });
    }
  }, [editingBook]);

  const handleDelete = async (bookId) => {
    try {
      await api.delete(`/books/${bookId}`, {
        headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
      });
      message.success("Kitap silindi");
      fetchUserBooks();
    } catch (err) {
      message.error("Kitap silinemedi");
    }
  };

  const handleEdit = (book) => {
    setEditingBook(book); // sadece bu yeterli
  };

  const handleUpdate = async (values) => {
    const formData = new FormData();
    formData.append("title", values.title);
    formData.append("type", values.type);
    formData.append("description", values.description);
    formData.append("imageUrl", values.imageUrl);
    formData.append("userId", localStorage.getItem("userId"));
    if (values.imageFile && values.imageFile.length > 0) {
        const file = values.imageFile[0].originFileObj;
        formData.append("imageFile", file);
      }

    try {
      await api.put(
        `/books/${editingBook.id}`,
        formData,
        {
          headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
        }
      );
      message.success("Kitap güncellendi");
      setEditingBook(null);
      fetchUserBooks();
    } catch (err) {
      message.error("Kitap güncellenemedi");
    }
  };

  return (
    <div style={{ padding: "20px" }}>
      <Title level={3}>Eklediğim Kitaplar</Title>
      <Row gutter={[16, 16]}>
        {books.map((book) => (
          <Col xs={24} sm={12} md={8} lg={5} key={book.id}>
            <Card
              hoverable
              cover={
                <img
                  alt={book.title}
                  src={
                    book.imageFile
                      ? `http://localhost:5217${book.imageFile}`
                      : book.imageUrl
                  }
                  style={{ height: 340, objectFit: "cover" }}
                />
              }
              actions={[
                <EditOutlined key="edit" onClick={() => handleEdit(book)} />,
                <DeleteOutlined key="delete" onClick={() => handleDelete(book.id)} />,
              ]}
            >
              <Card.Meta
                title={book.title}
                description={<Paragraph ellipsis={{ rows: 2 }}>{book.description}</Paragraph>}
              />
            </Card>
          </Col>
        ))}
      </Row>

      <Modal
        open={!!editingBook}
        title="Kitap Güncelle"
        onCancel={() => setEditingBook(null)}
        onOk={() => form.submit()}
      >
        <Form form={form} layout="vertical" onFinish={handleUpdate}>
          <Form.Item name="title" label="Kitap Başlığı" rules={[{ required: true }]}>
            <Input />
          </Form.Item>
          <Form.Item name="description" label="Açıklama">
            <Input.TextArea rows={3} />
          </Form.Item>
          <Form.Item name="type" label="Tür" rules={[{ required: true }]}>
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
            <Input />
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
        </Form>
      </Modal>
    </div>
  );
};

export default Account;